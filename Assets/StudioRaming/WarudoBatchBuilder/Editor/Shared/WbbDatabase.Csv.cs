using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbDatabase
    {
        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            char first = value[0];
            if (first == '=' || first == '+' || first == '-' || first == '@' || first == '\t' || first == '\r')
                value = "'" + value;
            bool needsQuote = value.IndexOf(',') >= 0 || value.IndexOf('"') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0;
            if (!needsQuote)
                return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public static bool ExportCsv(string absolutePath, out string error)
        {
            EnsureLoaded();
            error = "";
            if (string.IsNullOrEmpty(absolutePath))
            {
                error = "Export path is empty.";
                return false;
            }

            try
            {
                string dir = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var sb = new StringBuilder(1024 + records.Count * 256);
                for (int c = 0; c < CsvHeader.Length; c++)
                {
                    if (c > 0)
                        sb.Append(',');
                    sb.Append(CsvEscape(CsvHeader[c]));
                }

                sb.Append("\r\n");
                for (int i = 0; i < records.Count; i++)
                {
                    var r = records[i];
                    sb.Append(CsvEscape(r.id)).Append(',');
                    sb.Append(CsvEscape(r.startedIso)).Append(',');
                    sb.Append(CsvEscape(r.finishedIso)).Append(',');
                    sb.Append(CsvEscape(r.durationSec.ToString("0.###", CultureInfo.InvariantCulture))).Append(',');
                    sb.Append(CsvEscape(r.modName)).Append(',');
                    sb.Append(CsvEscape(((WbbExportType)r.exportType).ToString())).Append(',');
                    sb.Append(CsvEscape(r.modVersion)).Append(',');
                    sb.Append(CsvEscape(r.modAuthor)).Append(',');
                    sb.Append(CsvEscape(r.prefabGuid)).Append(',');
                    sb.Append(CsvEscape(r.prefabPath)).Append(',');
                    sb.Append(CsvEscape(r.prefabName)).Append(',');
                    sb.Append(CsvEscape(r.outputDirectory)).Append(',');
                    sb.Append(CsvEscape(r.outputFilePath)).Append(',');
                    sb.Append(CsvEscape(r.outputSizeBytes.ToString(CultureInfo.InvariantCulture))).Append(',');
                    sb.Append(CsvEscape(((WbbBuildStatus)r.status).ToString())).Append(',');
                    sb.Append(CsvEscape(r.message)).Append(',');
                    sb.Append(CsvEscape(r.jobId)).Append(',');
                    sb.Append(CsvEscape(r.jobName)).Append(',');
                    sb.Append(CsvEscape(r.sourceSignature)).Append(',');
                    sb.Append(CsvEscape(r.sourceObjectId)).Append(',');
                    sb.Append(CsvEscape(r.sourceScenePath)).Append(',');
                    sb.Append(CsvEscape(r.sourceHierarchyPath)).Append(',');
                    sb.Append(CsvEscape(r.fixupReport)).Append(',');
                    sb.Append(CsvEscape(r.buildLogPath));
                    sb.Append("\r\n");
                }

                File.WriteAllText(absolutePath, sb.ToString(), new UTF8Encoding(true));
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                WbbUtil.LogWarning("Failed to export CSV: " + absolutePath + " (" + e.Message + ")");
                return false;
            }
        }
    }
}
