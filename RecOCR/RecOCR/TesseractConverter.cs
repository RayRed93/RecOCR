using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Tesseract;

namespace RecOCR
{
    public class TesseractConverter
    {
        public enum OcrMode
        {
            alphanumeric,
            numeric
        }

        public class TessConvertResult
        {
            public string convertedText { get; set; }
            public float meanConfidence { get; set; }
            public TessConvertResult(string convertedText, float meanConfidence)
            {
                this.convertedText = convertedText;
                this.meanConfidence = meanConfidence;
            }
        }


        public static TessConvertResult BitmapToString(Bitmap bitmap, OcrMode mode)
        {
            string textResult;
            float confidence;

            if (bitmap.Size.IsEmpty) return null;          

            using (var engine = new TesseractEngine(@"./tessdata", "pol", EngineMode.Default))
            {
                if (mode == OcrMode.numeric)
                {
                    engine.SetVariable("tessedit_char_whitelist", "-0123456789.,"); 
                }
                Tesseract.BitmapToPixConverter converter = new BitmapToPixConverter();
               
                var imgPix = converter.Convert(bitmap);
                imgPix = imgPix.ConvertRGBToGray();
                imgPix = imgPix.Scale(7, 7);
                //imgPix.Save("test.tiff");
                
               
                using (var page = engine.Process(imgPix))
                {
                    textResult = page.GetText();
                    confidence = page.GetMeanConfidence();
                }

                if (confidence == 0 || textResult == "")
                {
                    return null;
                }
              
            }

            return new TessConvertResult(textResult, confidence);
        }
    }
}
