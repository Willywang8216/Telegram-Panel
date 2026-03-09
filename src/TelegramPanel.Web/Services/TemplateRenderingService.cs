using System.Text;
using System.Text.RegularExpressions;

namespace TelegramPanel.Web.Services;

/// <summary>
/// 模板变量解析服务。
/// </summary>
public sealed class TemplateRenderingService
{
    private static readonly Regex TokenRegex = new("\\{(?<name>[a-zA-Z0-9_]+)\\}", RegexOptions.Compiled);
    private readonly DataDictionaryService _dataDictionaryService;

    public TemplateRenderingService(DataDictionaryService dataDictionaryService)
    {
        _dataDictionaryService = dataDictionaryService;
    }

    public async Task<string> RenderTextTemplateAsync(string template, CancellationToken cancellationToken = default)
    {
        template = template ?? string.Empty;
        var matches = TokenRegex.Matches(template);
        if (matches.Count == 0)
            return template;

        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var builder = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            builder.Append(template, lastIndex, match.Index - lastIndex);
            var tokenName = match.Groups["name"].Value;
            if (!cache.TryGetValue(tokenName, out var resolved))
            {
                resolved = await ResolveTextTokenAsync(tokenName, cancellationToken);
                cache[tokenName] = resolved;
            }

            builder.Append(resolved);
            lastIndex = match.Index + match.Length;
        }

        builder.Append(template, lastIndex, template.Length - lastIndex);
        return builder.ToString();
    }

    public async Task<StoredImageAssetInfo> ResolveImageTemplateAsync(string tokenExpression, CancellationToken cancellationToken = default)
    {
        var tokenName = ExtractSingleTokenName(tokenExpression)
            ?? throw new InvalidOperationException("图片变量必须是单个字典变量，例如 {avatar}");
        return await _dataDictionaryService.ResolveImageValueAsync(tokenName, cancellationToken);
    }

    public string? ExtractSingleTokenName(string? tokenExpression)
    {
        var text = (tokenExpression ?? string.Empty).Trim();
        if (text.Length == 0)
            return null;

        var match = TokenRegex.Match(text);
        if (!match.Success || match.Index != 0 || match.Length != text.Length)
            return null;

        return match.Groups["name"].Value;
    }

    private async Task<string> ResolveTextTokenAsync(string tokenName, CancellationToken cancellationToken)
    {
        if (string.Equals(tokenName, "time", StringComparison.OrdinalIgnoreCase))
            return DateTime.Now.ToString("yyyyMMddHHmmss");

        return await _dataDictionaryService.ResolveTextValueAsync(tokenName, cancellationToken);
    }
}
