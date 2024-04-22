using System;
using System.IO;
using System.Text;
using Stimulsoft.Report;
using System.Xml;
using Stimulsoft.Report.Export;
using System.Drawing.Imaging;
using System.Globalization;
using Stimulsoft.Report.WCFService;

namespace WCFHelper
{
    public static class StiSLExportHelper
    {
        #region Methods
        public static byte[] StartExport(byte[] data)
        {
            using (var stream = new MemoryStream())
            using (var streamBinary = new MemoryStream(data))
            using (var reader = new StiBinaryReader(streamBinary))
            {
                var format = (StiExportFormat)Enum.Parse(typeof(StiExportFormat), reader.ReadNullableString());

                var report = new StiReport();
                report.LoadDocument(reader.ReadByteArray());

                switch (format)
                {
                    case StiExportFormat.Csv:
                        {
                            var settings = GetCsvExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Csv, stream, settings);
                        }
                        break;

                    case StiExportFormat.Dbf:
                        {
                            var settings = GetDbfExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Dbf, stream, settings);
                        }
                        break;

                    case StiExportFormat.Dif:
                        {
                            var settings = GetDifExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Dif, stream, settings);
                        }
                        break;

                    case StiExportFormat.Excel:
                        {
                            var settings = GetExcelExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Excel, stream, settings);
                        }
                        break;

                    case StiExportFormat.ExcelXml:
                        {
                            var settings = GetExcelXmlExportSettings(reader);
                            report.ExportDocument(StiExportFormat.ExcelXml, stream, settings);
                        }
                        break;

                    case StiExportFormat.Html:
                        {
                            var settings = GetHtmlExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Html, stream);
                        }
                        break;

                    case StiExportFormat.Html5:
                        {
                            var settings = GetHtml5ExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Html5, stream);
                        }
                        break;

                    case StiExportFormat.Mht:
                        {
                            var settings = GetMhtExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Mht, stream, settings);
                        }
                        break;

                    case StiExportFormat.Image:
                        {
                            var settings = CopyBitmapSettings(new StiBmpExportSettings(), GetImageExportSettings(reader));
                            report.ExportDocument(StiExportFormat.ImageBmp, stream, settings);
                        }
                        break;

                    case StiExportFormat.Ods:
                        {
                            var settings = GetOdsExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Ods, stream, settings);
                        }
                        break;

                    case StiExportFormat.Odt:
                        {
                            var settings = GetOdtExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Odt, stream, settings);
                        }
                        break;

                    case StiExportFormat.Pdf:
                        {
                            var pdfExportSettings = GetPdfExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Pdf, stream, pdfExportSettings);
                        }
                        break;

                    case StiExportFormat.Rtf:
                        {
                            var settings = GetRtfExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Rtf, stream, settings);
                        }
                        break;

                    case StiExportFormat.Sylk:
                        {
                            var settings = GetSylkExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Sylk, stream, settings);
                        }
                        break;

                    case StiExportFormat.Text:
                        {
                            var settings = GetTextExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Text, stream, settings);
                        }
                        break;

                    case StiExportFormat.Word:
                        {
                            var settings = GetWordExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Word, stream, settings);
                        }
                        break;

                    case StiExportFormat.Xps:
                        {
                            var settings = GetXpsExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Xps, stream, settings);
                        }
                        break;

                    case StiExportFormat.PowerPoint:
                        {
                            var settings = GetPowerPointExportSettings(reader);
                            report.ExportDocument(StiExportFormat.PowerPoint, stream, settings);
                        }
                        break;

                    case StiExportFormat.Xml:
                        {
                            var settings = GetXmlExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Xml, stream, settings);
                        }
                        break;

                    case StiExportFormat.Json:
                        {
                            var settings = GetJsonExportSettings(reader);
                            report.ExportDocument(StiExportFormat.Json, stream, settings);
                        }
                        break;
                }

                return stream.ToArray();
            }
        }

        private static StiImageExportSettings CopyBitmapSettings(StiImageExportSettings settings1, StiImageExportSettings settings2)
        {
            settings1.CutEdges = settings2.CutEdges;
            settings1.DitheringType = settings2.DitheringType;
            settings1.ImageFormat = settings2.ImageFormat;
            settings1.ImageResolution = settings2.ImageResolution;
            settings1.ImageZoom = settings2.ImageZoom;
            settings1.MultipleFiles = settings2.MultipleFiles;
            settings1.PageRange = settings2.PageRange;
            settings1.TiffCompressionScheme = settings2.TiffCompressionScheme;

            return settings1;
        }
        #endregion

        #region Methods.ParseExportSettings
        private static StiCsvExportSettings GetCsvExportSettings(StiBinaryReader reader)
        {
            var settings = new StiCsvExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.Encoding = ParseEncoding(reader.ReadInt32());
            settings.Separator = reader.ReadNullableString();
            settings.SkipColumnHeaders = reader.ReadBoolean();

            int mode = reader.ReadInt32();
            if (mode == 1) settings.DataExportMode = StiDataExportMode.DataAndHeadersFooters;
            if (mode == 2) settings.DataExportMode = StiDataExportMode.AllBands;

            return settings;
        }

        private static StiDbfExportSettings GetDbfExportSettings(StiBinaryReader reader)
        {
            var settings = new StiDbfExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.CodePage = (StiDbfCodePages)reader.ReadInt32();

            return settings;
        }

        private static StiDifExportSettings GetDifExportSettings(StiBinaryReader reader)
        {
            var settings = new StiDifExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.Encoding = ParseEncoding(reader.ReadInt32());
            settings.ExportDataOnly = reader.ReadBoolean();
            settings.UseDefaultSystemEncoding = reader.ReadBoolean();

            return settings;
        }

        private static StiExcelExportSettings GetExcelExportSettings(StiBinaryReader reader)
        {
            var settings = new StiExcelExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.UseOnePageHeaderAndFooter = reader.ReadBoolean();
            settings.DataExportMode = (StiDataExportMode)reader.ReadInt32();
            settings.ExportObjectFormatting = reader.ReadBoolean();
            settings.ExportEachPageToSheet = reader.ReadBoolean();
            settings.ExportPageBreaks = reader.ReadBoolean();
            settings.ImageResolution = reader.ReadSingle();
            settings.ImageQuality = reader.ReadSingle();
            settings.RestrictEditing = (StiExcel2007RestrictEditing)reader.ReadInt32();
            settings.EncryptionPassword = reader.ReadNullableString();
            settings.ProtectionPassword = reader.ReadNullableString();
            settings.ImageFormat = new ImageFormat(new Guid(reader.ReadNullableString()));
            settings.ImageResolutionMode = (StiImageResolutionMode)reader.ReadInt32();

            return settings;
        }

        private static StiExcelXmlExportSettings GetExcelXmlExportSettings(StiBinaryReader reader)
        {
            var settings = new StiExcelXmlExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.EncryptionPassword = reader.ReadNullableString();
            settings.ProtectionPassword = reader.ReadNullableString();

            return settings;
        }

        private static StiHtmlExportSettings GetHtmlExportSettings(StiBinaryReader reader)
        {
            var settings = new StiHtmlExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.Zoom = reader.ReadSingle();
            settings.ImageFormat = new ImageFormat(new Guid(reader.ReadNullableString()));
            settings.ExportMode = (StiHtmlExportMode)reader.ReadInt32();
            settings.ExportQuality = (StiHtmlExportQuality)reader.ReadInt32();
            settings.AddPageBreaks = reader.ReadBoolean();
            settings.CompressToArchive = reader.ReadBoolean();
            settings.UseEmbeddedImages = reader.ReadBoolean();

            return settings;
        }

        private static StiHtml5ExportSettings GetHtml5ExportSettings(StiBinaryReader reader)
        {
            var settings = new StiHtml5ExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ImageFormat = new ImageFormat(new Guid(reader.ReadNullableString()));
            settings.ImageResolution = reader.ReadSingle();
            settings.ImageQuality = reader.ReadSingle();
            settings.ContinuousPages = reader.ReadBoolean();
            settings.CompressToArchive = reader.ReadBoolean();

            return settings;
        }

        private static StiMhtExportSettings GetMhtExportSettings(StiBinaryReader reader)
        {
            var settings = new StiMhtExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.Zoom = reader.ReadSingle();
            settings.ImageFormat = new ImageFormat(new Guid(reader.ReadNullableString()));
            settings.ExportMode = (StiHtmlExportMode)reader.ReadInt32();
            settings.ExportQuality = (StiHtmlExportQuality)reader.ReadInt32();
            settings.AddPageBreaks = reader.ReadBoolean();
            settings.Encoding = Encoding.UTF8;

            return settings;
        }

        private static StiImageExportSettings GetImageExportSettings(StiBinaryReader reader)
        {
            var imageType = (StiImageType)reader.ReadInt32();
            var settings = new StiImageExportSettings(imageType);

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ImageZoom = reader.ReadDouble();
            settings.ImageResolution = reader.ReadInt32();
            settings.CutEdges = reader.ReadBoolean();
            settings.ImageFormat = (StiImageFormat)reader.ReadInt32();
            settings.MultipleFiles = reader.ReadBoolean();
            settings.TiffCompressionScheme = (StiTiffCompressionScheme)reader.ReadInt32();
            settings.DitheringType = (StiMonochromeDitheringType)reader.ReadInt32();
            settings.CompressToArchive = reader.ReadBoolean();

            return settings;
        }

        private static StiOdsExportSettings GetOdsExportSettings(StiBinaryReader reader)
        {
            var settings = new StiOdsExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ImageQuality = reader.ReadSingle();
            settings.ImageResolution = reader.ReadSingle();

            return settings;
        }

        private static StiOdtExportSettings GetOdtExportSettings(StiBinaryReader reader)
        {
            var settings = new StiOdtExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.UsePageHeadersAndFooters = reader.ReadBoolean();
            settings.ImageQuality = reader.ReadSingle();
            settings.ImageResolution = reader.ReadSingle();
            settings.RemoveEmptySpaceAtBottom = reader.ReadBoolean();

            return settings;
        }

        private static StiPdfExportSettings GetPdfExportSettings(StiBinaryReader reader)
        {
            var settings = new StiPdfExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ImageQuality = reader.ReadSingle();
            settings.ImageCompressionMethod = (StiPdfImageCompressionMethod)reader.ReadInt32();
            settings.ImageResolution = reader.ReadSingle();
            settings.EmbeddedFonts = reader.ReadBoolean();
            settings.ExportRtfTextAsImage = reader.ReadBoolean();
            settings.PasswordInputUser = reader.ReadNullableString();
            settings.PasswordInputOwner = reader.ReadNullableString();
            settings.UserAccessPrivileges = (StiUserAccessPrivileges)reader.ReadInt32();
            settings.KeyLength = (StiPdfEncryptionKeyLength)reader.ReadInt32();
            settings.GetCertificateFromCryptoUI = reader.ReadBoolean();
            settings.UseDigitalSignature = reader.ReadBoolean();
            settings.SubjectNameString = reader.ReadNullableString();
            settings.PdfComplianceMode = (StiPdfComplianceMode)reader.ReadInt32();
            settings.ImageFormat = (StiImageFormat)reader.ReadInt32();
            settings.DitheringType = (StiMonochromeDitheringType)reader.ReadInt32();
            settings.AllowEditable = (StiPdfAllowEditable)reader.ReadInt32();
            settings.ImageResolutionMode = (StiImageResolutionMode)reader.ReadInt32();
            settings.CertificateThumbprint = reader.ReadNullableString();

            return settings;
        }

        private static StiRtfExportSettings GetRtfExportSettings(StiBinaryReader reader)
        {
            var settings = new StiRtfExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ExportMode = (StiRtfExportMode)reader.ReadInt32();
            settings.UsePageHeadersAndFooters = reader.ReadBoolean();
            settings.ImageQuality = reader.ReadSingle();
            settings.ImageResolution = reader.ReadSingle();
            settings.RemoveEmptySpaceAtBottom = reader.ReadBoolean();

            return settings;
        }

        private static StiSylkExportSettings GetSylkExportSettings(StiBinaryReader reader)
        {
            var settings = new StiSylkExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ExportDataOnly = reader.ReadBoolean();
            settings.Encoding = ParseEncoding(reader.ReadInt32());
            settings.UseDefaultSystemEncoding = reader.ReadBoolean();

            return settings;
        }

        private static StiTxtExportSettings GetTextExportSettings(StiBinaryReader reader)
        {
            var settings = new StiTxtExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.Encoding = ParseEncoding(reader.ReadInt32());
            settings.DrawBorder = reader.ReadBoolean();
            settings.BorderType = (StiTxtBorderType)reader.ReadInt32();
            settings.KillSpaceLines = reader.ReadBoolean();
            settings.PutFeedPageCode = reader.ReadBoolean();
            settings.CutLongLines = reader.ReadBoolean();
            settings.ZoomX = reader.ReadSingle();
            settings.ZoomY = reader.ReadSingle();

            return settings;
        }

        private static StiWordExportSettings GetWordExportSettings(StiBinaryReader reader)
        {
            var settings = new StiWordExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.UsePageHeadersAndFooters = reader.ReadBoolean();
            settings.ImageQuality = reader.ReadSingle();
            settings.ImageResolution = reader.ReadSingle();
            settings.RemoveEmptySpaceAtBottom = reader.ReadBoolean();
            settings.RestrictEditing = (StiWord2007RestrictEditing)reader.ReadInt32();
            settings.EncryptionPassword = reader.ReadNullableString();
            settings.ProtectionPassword = reader.ReadNullableString();
            settings.ImageFormat = new ImageFormat(new Guid(reader.ReadNullableString()));
            settings.ImageResolutionMode = (StiImageResolutionMode)reader.ReadInt32();

            return settings;
        }

        private static StiXpsExportSettings GetXpsExportSettings(StiBinaryReader reader)
        {
            var settings = new StiXpsExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ImageQuality = reader.ReadSingle();
            settings.ImageResolution = reader.ReadSingle();
            settings.ExportRtfTextAsImage = reader.ReadBoolean();

            return settings;
        }

        private static StiPowerPointExportSettings GetPowerPointExportSettings(StiBinaryReader reader)
        {
            var settings = new StiPowerPointExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.ImageResolution = reader.ReadSingle();
            settings.ImageQuality = reader.ReadSingle();
            settings.EncryptionPassword = reader.ReadNullableString();
            settings.ImageResolutionMode = (StiImageResolutionMode)reader.ReadInt32();
            settings.ImageFormat = new ImageFormat(new Guid(reader.ReadNullableString()));

            return settings;
        }

        private static StiXmlExportSettings GetXmlExportSettings(StiBinaryReader reader)
        {
            var settings = new StiXmlExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.DataExportMode = (StiDataExportMode)reader.ReadInt32();

            return settings;
        }

        private static StiJsonExportSettings GetJsonExportSettings(StiBinaryReader reader)
        {
            var settings = new StiJsonExportSettings();

            settings.PageRange.CurrentPage = reader.ReadInt32();
            settings.PageRange.PageRanges = reader.ReadNullableString();
            settings.PageRange.RangeType = (StiRangeType)reader.ReadInt32();

            settings.DataExportMode = (StiDataExportMode)reader.ReadInt32();

            return settings;
        }
        #endregion

        #region Methods.ParseProperties
        private static Encoding ParseEncoding(int codepage)
        {
            return Encoding.GetEncoding(codepage);
        }
        #endregion
    }
}