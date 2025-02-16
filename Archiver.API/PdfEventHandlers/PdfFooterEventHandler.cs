using iText.Commons.Actions;
using iText.Kernel.Exceptions;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using System.Drawing;

namespace Archiver.API.PdfEventHandlers
{
    public class PdfFooterEventHandler : AbstractPdfDocumentEventHandler
    {
        
        protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
        {
            var ev = (PdfDocumentEvent)@event;
            var page = ev.GetPage();
            var doc = ev.GetDocument();
            var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), doc);
            int pagenum = doc.GetPageNumber(page);
            var pageSize = page.GetPageSize();

            float x = pageSize.GetRight() - 30f;
            float y = pageSize.GetBottom() + 15f;

            new Canvas(page, pageSize)
                .ShowTextAligned($"page {pagenum}", x, y, iText.Layout.Properties.TextAlignment.RIGHT)
                .Close();
        }
    }
}
