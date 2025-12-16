# Secure API Key Configuration

Your OpenAI API key is now protected and won't be checked into source control!

## Quick Setup

1. **Edit the file** `appsettings.Development.json` (already created for you)
2. **Add your API key**:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-your-actual-openai-api-key-here"
     }
   }
   ```
3. **Run the tool** - no need to specify `--api-key`:
   ```powershell
   dotnet run -- package1.nupkg package2.nupkg --migration-guide ./guides
   ```

## How It Works

The tool checks for your API key in this order:
1. `--api-key` command line parameter (highest priority)
2. `OPENAI_API_KEY` environment variable
3. `appsettings.Development.json` configuration file

## Security

✅ **Protected**: `appsettings.Development.json` is in `.gitignore` and won't be committed
✅ **Safe**: Your API key stays on your local machine
✅ **Convenient**: No need to type it every time

## Alternative Methods

### Environment Variable (session-based)
```powershell
$env:OPENAI_API_KEY = "sk-your-key"
dotnet run -- package1.nupkg package2.nupkg --migration-guide ./guides
```

### Command Line (one-time use)
```powershell
dotnet run -- package1.nupkg package2.nupkg --api-key sk-your-key --migration-guide ./guides
```

## Get Your API Key

Visit: https://platform.openai.com/api-keys
