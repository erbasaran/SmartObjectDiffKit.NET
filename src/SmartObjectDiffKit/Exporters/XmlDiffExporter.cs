using System.Text;
using System.Xml;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to XML format.
/// </summary>
public sealed class XmlDiffExporter : IDiffExporter
{
    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false
        };

        using var writer = XmlWriter.Create(sb, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("DiffResult");

        writer.WriteElementString("IsEqual", result.IsEqual.ToString());
        writer.WriteElementString("DifferenceCount", result.DifferenceCount.ToString());
        writer.WriteElementString("ElapsedTimeMs", result.ElapsedTime.TotalMilliseconds.ToString("F2"));
        writer.WriteElementString("ComparedObjectCount", result.ComparedObjectCount.ToString());
        writer.WriteElementString("ComparedPropertyCount", result.ComparedPropertyCount.ToString());
        writer.WriteElementString("MaximumDepth", result.MaximumDepth.ToString());

        writer.WriteStartElement("Differences");
        foreach (var diff in result.Differences)
        {
            writer.WriteStartElement("Difference");
            writer.WriteElementString("PropertyPath", diff.PropertyPath);
            writer.WriteElementString("OldValue", diff.OldValue?.ToString());
            writer.WriteElementString("NewValue", diff.NewValue?.ToString());
            writer.WriteElementString("OldType", diff.OldType?.Name);
            writer.WriteElementString("NewType", diff.NewType?.Name);
            writer.WriteElementString("DifferenceType", diff.DifferenceType.ToString());
            writer.WriteElementString("Severity", diff.Severity.ToString());
            writer.WriteElementString("Depth", diff.Depth.ToString());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Statistics");
        writer.WriteElementString("VisitedObjects", result.Statistics.VisitedObjects.ToString());
        writer.WriteElementString("ComparedProperties", result.Statistics.ComparedProperties.ToString());
        writer.WriteElementString("ComparedCollections", result.Statistics.ComparedCollections.ToString());
        writer.WriteElementString("MaximumDepthReached", result.Statistics.MaximumDepthReached.ToString());
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        return sb.ToString();
    }
}
