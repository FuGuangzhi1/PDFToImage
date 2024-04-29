

using PDFiumCore;

using SixLabors.ImageSharp;

using static PDFiumCore.fpdfview;

namespace WebApplication1.Helper.PDF;

public static class PDFHelpe
{
    static PDFHelpe()
    {
        FPDF_InitLibrary();
    }
    public static unsafe byte[][] PDFToPng(this Stream stream)
    {
        try
        {
            using MemoryStream ms = new();
            ms.Position = default;
            stream.CopyTo(ms);
            byte[] fileBytes = ms.ToArray();

            double pageWidth = default;
            double pageHeight = default;
            float scale = 1;
            // White color.
            uint color = uint.MaxValue;
            fixed (void* p = fileBytes)
            {
                //_ = FPDF_GetLastError();
                // Load the document.
                FpdfDocumentT document = FPDF_LoadMemDocument(new IntPtr(p), fileBytes.Length, string.Empty);
                //FPDF_CloseDocument(document);
                int pageSize = FPDF_GetPageCount(document);
                byte[][] bytes = new byte[pageSize][];
                for (int index = 0; index < pageSize; index++)
                {
                    FpdfPageT page = FPDF_LoadPage(document, index);
                    _ = FPDF_GetPageSizeByIndex(document, index, ref pageWidth, ref pageHeight);

                    Rectangle viewport = new()
                    {
                        X = default,
                        Y = default,
                        Width = pageWidth,
                        Height = pageHeight,
                    };
                    FpdfBitmapT bitmap = FPDFBitmapCreateEx(
                            (int)viewport.Width,
                            (int)viewport.Height,
                            (int)FPDFBitmapFormat.BGRA,
                            nint.Zero,
                            default) ?? throw new Exception("failed to create a bitmap object");

                    // Leave out if you want to make the background transparent.
                    FPDFBitmapFillRect(bitmap, default, default, (int)viewport.Width, (int)viewport.Height, color);

                    // |          | a b 0 |
                    // | matrix = | c d 0 |
                    // |          | e f 1 |
                    using FS_MATRIX_ matrix = new();
                    using FS_RECTF_ clipping = new();

                    matrix.A = scale;
                    matrix.B = default;
                    matrix.C = default;
                    matrix.D = scale;
                    matrix.E = (float)-viewport.X;
                    matrix.F = (float)-viewport.Y;

                    clipping.Left = default;
                    clipping.Right = (int)viewport.Width;
                    clipping.Bottom = default;
                    clipping.Top = (float)viewport.Height;

                    FPDF_RenderPageBitmapWithMatrix(bitmap, page, matrix, clipping, (int)RenderFlags.RenderAnnotations);


                    PdfImage image = new(
                        bitmap,
                        (int)pageWidth,
                        (int)pageHeight);
                    using MemoryStream ms1 = new();
                    image.ImageData.SaveAsPng(ms1);
                    bytes[index] = ms1.ToArray();
                }
                return bytes;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            _ = FPDF_GetLastError();
            return [];
        }
    }
}

