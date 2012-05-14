using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Drawing;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Threading;

namespace PrintomiImageCleaner
{
    class Program
    {
        static int tileW = 225;
        static int tileH = 168;
        static int imagesW = 4;
        static int imagesH = 4;
        static int numImages = imagesW * imagesH;
        static string exportDir = @"D:\PrintomiMaps\input\";
        static string infosFile = @"D:\PrintomiMaps\input\infos.txt";
        static string outputDir = @"D:\PrintomiMaps\output\";       

        static void Main(string[] args)
        {
            // Pull in all the images from the json DB dump
            var allInfos = File.ReadAllText(infosFile).Split("\n\n".ToCharArray()).ToList();
            allInfos.RemoveAll(s => s == "");

            // Convert to JSON
            var jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
            var allJson = allInfos.ConvertAll(i=>jss.Deserialize(i, typeof(object)) as dynamic);        

            // Remove any that dont have a file in the input folder
            allJson.RemoveAll(d=> !File.Exists(string.Format("{0}{1}.jpg",exportDir,d.Id)));
            allJson = allJson.Take(numImages).ToList();

            StitchUtils.toolsDir = Path.GetFullPath("./tools/");

            // Recursively build up the levels > the width of a singe tile
            var depthReached = recurseMakeLevels(allJson,0,0,0);

            // Do the rest of this in parallel
            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 6;
            Parallel.For(0, allJson.Count, options, i =>
            {
                var o = allJson[i];
                Console.WriteLine("Starting: " + i);
                Image img = Image.FromFile(string.Format("{0}{1}.jpg", exportDir, o.Id));
                ExportChunks(img, 5 + depthReached, i % imagesW, i / imagesW);
                img = ImageUtilities.ResizeImage(img, tileW * 8, tileH*8);
                ExportChunks(img, 4 + depthReached, i % imagesW, i / imagesW);
                img = ImageUtilities.ResizeImage(img, tileW * 4, tileH*4);
                ExportChunks(img, 3 + depthReached, i % imagesW, i / imagesW);
                img = ImageUtilities.ResizeImage(img, tileW * 2, tileH*2);
                ExportChunks(img, 2 + depthReached, i % imagesW, i / imagesW);
                img = ImageUtilities.ResizeImage(img, tileW, tileH);
                ExportChunks(img, 1 + depthReached, i % imagesW, i / imagesW);
                Console.WriteLine(string.Format("Finished: {0} ({1}%)", i, (int)((i / (double)numImages) * 100)));
            });     
     
        }

        private static int recurseMakeLevels(List<dynamic> images, int x, int y, int z)
        {
            var i = putImagesOnATile(images);
            var f = string.Format("{0}{1}_{2}_{3}.jpg", outputDir, x, y, z);
            ImageUtilities.SaveJpeg(f, i, 90);
            StitchUtils.Optimize(f, f);
            i.Dispose();
            i = null;

            if (images.Count > 4)
            {
                int perSide = ((int)Math.Sqrt(images.Count))/2;        
                recurseMakeLevels(getQuadrant(images, perSide, perSide, 0, 0), x*2, y*2, z+1);
                recurseMakeLevels(getQuadrant(images, perSide, perSide, perSide, 0), 1 + x * 2, y*2, z + 1);
                recurseMakeLevels(getQuadrant(images, perSide, perSide, 0, perSide), x * 2, 1+y*2, z + 1);
                return recurseMakeLevels(getQuadrant(images, perSide, perSide, perSide, perSide), 1 + x * 2, 1+y*2, z + 1);                
            }
            return z;
        }

        private static List<dynamic> getQuadrant(List<dynamic> images, int w, int h, int xoff, int yoff)
        {
            var l = new List<dynamic>();
            for (var yi = yoff; yi < h + yoff; yi++)
            {
                for (var xi = xoff; xi < w + xoff; xi++)
                {
                    l.Add(images[xi + yi * (w*2)]);
                }
            }
            return l;
        }

        private static Image putImagesOnATile(List<dynamic> images)
        {
            int perSide = (int)Math.Sqrt(images.Count);
           
            var outImg = new Bitmap(tileW, tileH);            
            var g = Graphics.FromImage(outImg);
        
            for (var yi = 0; yi < perSide; yi++)
            {
                for (var xi = 0; xi < perSide; xi++)
                {
                    var o = images[xi + yi * perSide];
                    var i = Image.FromFile(string.Format("{0}{1}.jpg", exportDir, o.Id));
                    var tw = (float)tileW / perSide;
                    var th = (float)tileH / perSide;
                    //i = ImageUtilities.ResizeImage(i, (tileW / perSide)+1, (tileH / perSide)+1);
                    g.DrawImage(i, new Rectangle((int)(xi * tw), (int)(yi * th), (tileW / perSide) + 1, (tileH / perSide) + 1), new Rectangle(0,0,i.Width,i.Height), GraphicsUnit.Pixel);
                    i.Dispose();                   
                }
            }

            g.Dispose();
            return outImg;
        }

        private static void ExportChunks(Image i, int level, int imageX, int imageY)
        {
            Console.WriteLine(string.Format("Exporting Chunks. {0},{1},{2}", imageX, imageY, level));
        
            int numInX = i.Width / tileW;
            int numInY = i.Height / tileH;

            for (var yi = 0; yi < numInY; yi++)
            {
                for (var xi = 0; xi < numInX; xi++)
                {
                    var chunk = new Bitmap(tileW, tileH);
                    var g = Graphics.FromImage(chunk);
                    g.DrawImage(i, new Point(-xi * chunk.Width, -yi * chunk.Height));
                    var f = string.Format("{0}{1}_{2}_{3}.jpg", outputDir,(numInX * imageX) + xi, (numInY * imageY) + yi, level);
                    ImageUtilities.SaveJpeg(f, chunk, 90);                    
                    StitchUtils.Optimize(f, f);                  
                }
            }

            if (i.Width == tileW && i.Height == tileH)
            {
                var f = string.Format("{0}{1}_{2}_{3}.jpg",outputDir, imageX, imageY, level);
                ImageUtilities.SaveJpeg(f, i, 90);
                StitchUtils.Optimize(f, f);
            }
        }
    }
}
