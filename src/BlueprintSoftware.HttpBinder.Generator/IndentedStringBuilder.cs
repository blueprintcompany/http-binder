using System.Text;

namespace BlueprintSoftware.HttpBinder;

/// <summary>
/// Simple utility class to assist with indenting generated source code.
/// Maintains an internal <see cref="StringBuilder"/> and tracks the
/// current indentation level. Each call to <see cref="Indent"/> or
/// <see cref="Unindent"/> adjusts the indentation prefix used when
/// appending new lines.
/// </summary>
internal sealed class IndentedStringBuilder(StringBuilder sb, int initialIndentLevel = 0, string indentToken = "    ")
{
    private int _indentLevel = initialIndentLevel;

    public void Indent() => _indentLevel++;
    public void Unindent() => _indentLevel = _indentLevel > 0 ? _indentLevel - 1 : 0;

    public void Append(string text)
    {
        sb.Append(text);
    }

    public void AppendLine(string text)
    {
        AppendIndent();
        sb.AppendLine(text);
    }

    public void AppendLine()
    {
        sb.AppendLine();
    }

    private void AppendIndent()
    {
        for (var i = 0; i < _indentLevel; i++)
        {
            sb.Append(indentToken);
        }
    }
}