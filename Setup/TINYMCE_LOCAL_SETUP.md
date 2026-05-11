# TinyMCE Local Installation Guide

## Overview
This guide walks you through installing TinyMCE locally instead of using the CDN version.

## Benefits of Local Installation
✅ Works offline
✅ Faster loading (no external requests)
✅ Full control over versions
✅ No dependency on CDN availability
✅ Better for production deployments

## Installation Steps

### Step 1: Download TinyMCE

**Option A: Download from Official Website**
1. Visit https://www.tiny.cloud/tinymce/
2. Create a free account or log in
3. Download the latest version (v6 or later recommended)
4. Extract the ZIP file

**Option B: Use npm**
```bash
npm install tinymce
# Then copy node_modules/tinymce to your project
```

**Option C: Direct Download**
- Download from: https://github.com/tinymce/tinymce-dist/releases
- Extract the latest release

### Step 2: Create Directory Structure

Create this folder in your project:
```
LuxfordPTAWeb\wwwroot\lib\tinymce\
```

### Step 3: Copy TinyMCE Files

Copy the entire TinyMCE distribution to `wwwroot\lib\tinymce\`

Your final structure should look like:
```
LuxfordPTAWeb\
  wwwroot\
    lib\
      tinymce\
        js\
          tinymce.min.js
          tinymce.js
        themes\
          silver\
            theme.min.js
        plugins\
          anchor\
          autolink\
          charmap\
          code\
          codesample\
          emoticons\
          image\
          insertdatetime\
          link\
          lists\
          media\
          nonbreaking\
          preview\
          searchreplace\
          table\
          visualblocks\
          wordcount\
        icons\
          default\
        skins\
          silver\
        models\
        license.txt
```

### Step 4: Verify Installation

- Check that `wwwroot/lib/tinymce/tinymce.min.js` exists
- The file should be approximately 500-600KB in size

### Step 5: Build and Test

1. Build the project:
   ```bash
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Navigate to `/admin/programs` and create or edit a program
4. The TinyMCE editor should load in the "Long Description" field

## Configuration

The TinyMCE configuration is in:
```
LuxfordPTAWeb\wwwroot\js\tinymce-interop.js
```

You can modify the toolbar, plugins, and other settings there.

### Common Customizations

**Change toolbar:**
```javascript
toolbar: 'undo redo | blocks | bold italic | link image table | removeformat'
```

**Add more plugins:**
```javascript
plugins: [
    'anchor', 'autolink', 'charmap', 'codesample', 'emoticons', 
    'image', 'insertdatetime', 'link', 'lists', 'media',
    'nonbreaking', 'preview', 'searchreplace', 'table', 
    'visualblocks', 'wordcount', 'pagebreak', 'help'
],
```

**Change editor height:**
```javascript
window.initTinyMCE = function(height) {
    tinymce.init({
        height: height || 400, // Default 400px if not specified
        // ... rest of config
    });
};
```

## Using TinyMCE in Components

To use TinyMCE in other Blazor components:

```razor
@using LuxfordPTAWeb.Client.Components

<TinyMCEEditor @bind-Value="content" Height="500" />
```

## Troubleshooting

**Issue: "TinyMCE is not defined"**
- Solution: Make sure `tinymce.min.js` is properly loaded
- Check browser console for any errors
- Verify file path in App.razor matches actual location

**Issue: Plugins not loading**
- Solution: Verify the `plugins` folder exists in `wwwroot/lib/tinymce/plugins/`
- Check that plugin folder names match plugin names in configuration

**Issue: Icons not displaying**
- Solution: Verify `icons` folder exists in `wwwroot/lib/tinymce/icons/`
- Check console for 404 errors

**Issue: Themes not applying**
- Solution: Verify `themes` folder exists
- The default theme is "silver"

## Version Update

To update TinyMCE in the future:
1. Download the new version
2. Delete the old `wwwroot/lib/tinymce/` folder
3. Copy the new version there
4. Rebuild the application

## Performance Tips

1. **Minified Files**: Use `.min.js` files for production
2. **Lazy Loading**: TinyMCE only loads when the component is rendered
3. **Caching**: Static files are cached by browsers
4. **File Size**: Typical installation is 1-2MB (gzipped)

## License

TinyMCE includes a free GPL license (`license.txt` file).

For commercial use or to remove the GPL notice, you can:
- Upgrade to a commercial license
- Use the free TinyMCE Cloud with a custom API key

## Next Steps

1. Download TinyMCE using one of the options above
2. Create the `wwwroot\lib\tinymce\` directory
3. Extract TinyMCE files there
4. Build and test the application
5. Customize the toolbar/plugins as needed

Your local TinyMCE installation is now ready! 🎉
