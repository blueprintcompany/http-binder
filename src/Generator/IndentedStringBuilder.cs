using System.Text;

namespace Blueprint.HttpBinder;

/// <summary>
/// Simple utility class to assist with indenting generated source code.
/// Maintains an internal <see cref="StringBuilder"/> and tracks the
/// current indentation level. Each call to <see cref="Indent"/> or
/// <see cref="Unindent"/> adjusts the indentation prefix used when
/// appending new lines.
/// </summary>
internal sealed class IndentedStringBuilder(StringBuilder sb, int initialIndentLevel = 0, string indentToken = "    ")
{
    private readonly StringBuilder _sb = sb;
    private readonly string _indentToken = indentToken;
    private int _indentLevel = initialIndentLevel;

    public void Indent() => _indentLevel++;
    public void Unindent() => _indentLevel = _indentLevel > 0 ? _indentLevel - 1 : 0;

    public void Append(string text)
    {
        _sb.Append(text);
    }

    public void AppendLine(string text)
    {
        AppendIndent();
        _sb.AppendLine(text);
    }

    public void AppendLine()
    {
        _sb.AppendLine();
    }

    private void AppendIndent()
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append(_indentToken);
        }
    }
}