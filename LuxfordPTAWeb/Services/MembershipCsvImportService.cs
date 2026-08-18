using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Services;

public class MembershipCsvImportService : IMembershipCsvImportService
{
    private readonly ApplicationDbContext _db;

    public MembershipCsvImportService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Canonical field -> accepted header aliases (matched case/space/punctuation-insensitively),
    // so we can accept CSVs exported from different membership tools without a fixed schema.
    private static readonly Dictionary<string, string[]> FieldAliases = new()
    {
        ["FirstName"] = ["firstname", "first", "fname"],
        ["LastName"] = ["lastname", "last", "lname"],
        ["Email"] = ["email", "emailaddress", "e-mail"],
        ["PhoneNumber"] = ["phonenumber", "phone", "cellphone", "phone#", "cell"],
        ["JoinDate"] = ["joindate", "datejoined", "date", "joined"],
        ["MemberType"] = ["membertype", "type", "role"],
        ["Price"] = ["price", "amount", "amountpaid", "dues"],
        ["Status"] = ["status", "memberstatus"],
        ["PaymentType"] = ["paymenttype", "payment", "paymentmethod"],
        ["TeacherName"] = ["teachername", "teacher", "homeroom"]
    };

    private static string Normalize(string s) =>
        new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    /// <summary>
    /// Identity key for duplicate detection: email if present, otherwise phone number (digits only)
    /// so members with no email on file still get deduped. Null means we have no reliable identifier
    /// at all — such a row is never flagged as a duplicate.
    /// </summary>
    private static string? GetDedupeKey(string? email, string? phone)
    {
        if (!string.IsNullOrWhiteSpace(email)) return "email:" + email.Trim().ToLowerInvariant();
        var digits = new string((phone ?? "").Where(char.IsDigit).ToArray());
        return digits.Length > 0 ? "phone:" + digits : null;
    }

    public async Task<MembershipImportPreviewDTO> ParsePreviewAsync(Stream csvStream, int schoolYearId)
    {
        var preview = new MembershipImportPreviewDTO { SchoolYearId = schoolYearId };

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            BadDataFound = null
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);

        if (!await csv.ReadAsync() || !csv.ReadHeader() || csv.HeaderRecord == null)
        {
            preview.FileErrors.Add("Could not read a header row from the file. Make sure the first row contains column names.");
            return preview;
        }

        // Map each canonical field to the actual header text present in this file.
        var normalizedHeaders = csv.HeaderRecord.ToDictionary(Normalize, h => h);
        var columnMap = new Dictionary<string, string?>();
        foreach (var (canonical, aliases) in FieldAliases)
        {
            var match = aliases.Select(a => normalizedHeaders.GetValueOrDefault(a)).FirstOrDefault(h => h != null)
                        ?? normalizedHeaders.GetValueOrDefault(Normalize(canonical));
            columnMap[canonical] = match;
        }

        if (columnMap["FirstName"] == null || columnMap["LastName"] == null)
        {
            preview.FileErrors.Add("The file must include First Name and Last Name columns.");
            return preview;
        }
        if (columnMap["Email"] == null && columnMap["PhoneNumber"] == null)
        {
            preview.FileErrors.Add("The file must include an Email or a Phone Number column.");
            return preview;
        }

        var existingKeys = new HashSet<string>(
            (await _db.MembershipRecords
                .Where(mr => mr.SchoolYearId == schoolYearId)
                .Select(mr => new { mr.Email, mr.PhoneNumber })
                .ToListAsync())
            .Select(r => GetDedupeKey(r.Email, r.PhoneNumber))
            .Where(k => k != null)!);

        var seenInFile = new HashSet<string>();
        int rowNumber = 1; // header is row 1

        while (await csv.ReadAsync())
        {
            rowNumber++;
            var row = new MembershipImportRowDTO { RowNumber = rowNumber };

            row.FirstName = GetField(csv, columnMap, "FirstName");
            row.LastName = GetField(csv, columnMap, "LastName");
            row.Email = GetField(csv, columnMap, "Email").Trim();
            row.PhoneNumber = GetField(csv, columnMap, "PhoneNumber");
            row.MemberType = GetField(csv, columnMap, "MemberType");
            row.Status = GetField(csv, columnMap, "Status");
            row.PaymentType = GetField(csv, columnMap, "PaymentType");
            row.TeacherName = GetField(csv, columnMap, "TeacherName");

            var joinDateRaw = GetField(csv, columnMap, "JoinDate");
            if (!string.IsNullOrWhiteSpace(joinDateRaw))
            {
                if (DateTime.TryParse(joinDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var joinDate))
                    row.JoinDate = joinDate;
                else
                    row.Errors.Add($"Could not parse join date '{joinDateRaw}'.");
            }

            var priceRaw = GetField(csv, columnMap, "Price").Replace("$", "").Trim();
            if (!string.IsNullOrWhiteSpace(priceRaw))
            {
                if (decimal.TryParse(priceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
                    row.Price = price;
                else
                    row.Errors.Add($"Could not parse price '{priceRaw}'.");
            }

            if (string.IsNullOrWhiteSpace(row.FirstName)) row.Errors.Add("First name is required.");
            if (string.IsNullOrWhiteSpace(row.LastName)) row.Errors.Add("Last name is required.");
            if (string.IsNullOrWhiteSpace(row.Email) && string.IsNullOrWhiteSpace(row.PhoneNumber))
                row.Errors.Add("Either an email or a phone number is required.");
            else if (!string.IsNullOrWhiteSpace(row.Email) && !row.Email.Contains('@'))
                row.Errors.Add($"Email '{row.Email}' doesn't look valid.");

            var dedupeKey = GetDedupeKey(row.Email, row.PhoneNumber);
            if (dedupeKey != null)
            {
                row.IsDuplicate = existingKeys.Contains(dedupeKey) || !seenInFile.Add(dedupeKey);
            }

            row.Include = row.Errors.Count == 0;
            preview.Rows.Add(row);
        }

        return preview;
    }

    private static string GetField(CsvReader csv, Dictionary<string, string?> columnMap, string canonicalField)
    {
        var header = columnMap[canonicalField];
        if (header == null) return string.Empty;
        return csv.TryGetField<string>(header, out var value) ? value ?? string.Empty : string.Empty;
    }

    public async Task<MembershipImportResultDTO> CommitAsync(MembershipImportCommitDTO commit)
    {
        var result = new MembershipImportResultDTO();

        var schoolYearExists = await _db.SchoolYears.AnyAsync(sy => sy.Id == commit.SchoolYearId);
        if (!schoolYearExists)
        {
            result.Errors.Add("Selected school year no longer exists.");
            return result;
        }

        // Built with a plain loop (not ToDictionaryAsync) so pre-existing rows that happen to
        // share a dedupe key (e.g. two records with no email and no phone) don't crash the commit —
        // the first match for a key wins and later ones are treated as having no reliable identity.
        var existing = new Dictionary<string, MembershipRecord>();
        foreach (var r in await _db.MembershipRecords.Where(mr => mr.SchoolYearId == commit.SchoolYearId).ToListAsync())
        {
            var key = GetDedupeKey(r.Email, r.PhoneNumber);
            if (key != null) existing.TryAdd(key, r);
        }

        foreach (var row in commit.Rows)
        {
            if (!row.Include)
            {
                result.Skipped++;
                continue;
            }

            if (row.Errors.Count > 0)
            {
                result.Errors.Add($"Row {row.RowNumber}: skipped due to validation errors.");
                result.Skipped++;
                continue;
            }

            var dedupeKey = GetDedupeKey(row.Email, row.PhoneNumber);
            var isStaff = row.MemberType.Contains("staff", StringComparison.OrdinalIgnoreCase)
                          || row.MemberType.Contains("teacher", StringComparison.OrdinalIgnoreCase);
            var status = row.Status.Trim().Equals("Inactive", StringComparison.OrdinalIgnoreCase)
                ? MembershipStatus.Inactive
                : MembershipStatus.Active;

            if (dedupeKey != null && existing.TryGetValue(dedupeKey, out var existingRecord))
            {
                if (commit.DuplicateStrategy == DuplicateImportStrategy.Skip)
                {
                    result.Skipped++;
                    continue;
                }

                existingRecord.FirstName = row.FirstName;
                existingRecord.LastName = row.LastName;
                existingRecord.PhoneNumber = row.PhoneNumber;
                existingRecord.JoinDate = row.JoinDate ?? existingRecord.JoinDate;
                existingRecord.MemberType = row.MemberType;
                existingRecord.Price = row.Price;
                existingRecord.Status = status;
                existingRecord.PaymentType = row.PaymentType;
                existingRecord.TeacherName = row.TeacherName;
                existingRecord.IsStaff = isStaff;
                existingRecord.UpdatedAt = DateTime.UtcNow;
                result.Updated++;
            }
            else
            {
                var record = new MembershipRecord
                {
                    Id = Guid.NewGuid(),
                    SchoolYearId = commit.SchoolYearId,
                    FirstName = row.FirstName,
                    LastName = row.LastName,
                    Email = row.Email,
                    PhoneNumber = row.PhoneNumber,
                    JoinDate = row.JoinDate ?? DateTime.Today,
                    MemberType = row.MemberType,
                    Price = row.Price,
                    Status = status,
                    PaymentType = row.PaymentType,
                    TeacherName = row.TeacherName,
                    IsStaff = isStaff,
                    CreatedAt = DateTime.UtcNow
                };
                _db.MembershipRecords.Add(record);
                if (dedupeKey != null) existing[dedupeKey] = record; // guard against duplicates within the same commit
                result.Imported++;
            }
        }

        await _db.SaveChangesAsync();
        return result;
    }
}
