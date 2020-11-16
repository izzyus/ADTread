using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using WoWFormatLib.Structs.ADT;
namespace Generators.ADT_Alpha
{
    class ADT_Alpha
    {

        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------

        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();
        public List<Bitmap> TextureLayers = new List<Bitmap>();

        //-----------------------------------------------------------------------------------------------------------------

        public void GenerateAlphaMaps(ADT adtfile)
        {
            #region Splatmap Bitmap
            //Chunk offset
            int xOff = 0;
            int yOff = 0;

            //How many splatmaps we need:
            int neededSplatmaps = IntCeil(adtfile.textures.filenames.Length, 4);
            //Console.WriteLine(String.Format("We need {0} spaltmaps.", neededSplatmaps));
            Bitmap[] splatmaps = new Bitmap[neededSplatmaps];
            //Initialize the bitmaps
            for (int i = 0; i < splatmaps.Length; i++)
            {
                splatmaps[i] = new Bitmap(1024, 1024);
            }


            Bitmap[] textureLayers = new Bitmap[adtfile.textures.filenames.Length];
            //Initialize the bitmaps
            for (int i = 0; i < adtfile.textures.filenames.Length; i++)
            {
                textureLayers[i] = new Bitmap(1024, 1024);
            }


            //Map textures to their respective splatmap
            Dictionary<int, int> textureChannelIndex = new Dictionary<int, int>();
            Dictionary<int, int> textureSplatmapIndex = new Dictionary<int, int>();
            for (int textureIndex = 0, splatmapIndex = 0, channelIndex = 0; textureIndex < adtfile.textures.filenames.Length; textureIndex++)
            {
                //string textureName = adtfile.textures.filenames[textureIndex].ToLower(); //Dropped, we are using texture indices now
                textureChannelIndex.Add(textureIndex, channelIndex);
                textureSplatmapIndex.Add(textureIndex, splatmapIndex);
                //Console.WriteLine(string.Format("Tex {0}; Splat: {1}; Chn: {2}", textureIndex, splatmapIndex, channelIndex));
                if (channelIndex == 3)
                {
                    channelIndex = 0;
                    splatmapIndex++;
                }
                else
                {
                    channelIndex++;
                }
            }


            //Loop for all the chunks
            for (uint c = 0; c < adtfile.chunks.Count(); c++)
            {
                for (int li = 1; li < adtfile.texChunks[c].layers.Count(); li++) //we start at 1, we do not care about layer 0
                {
                    if (adtfile.texChunks[c].alphaLayer != null)
                    {
                        var values = adtfile.texChunks[c].alphaLayer[li].layer;
                        var si = textureSplatmapIndex[(int)adtfile.texChunks[c].layers[li].textureId];
                        var chi = textureChannelIndex[(int)adtfile.texChunks[c].layers[li].textureId];

                        for (int x = 0; x < 64; x++)
                        {
                            for (int y = 0; y < 64; y++)
                            {
                                var currentColor = new Color();
                                var existingPixel = splatmaps[si].GetPixel(x + xOff, y + yOff);
                                switch (chi)
                                {
                                    case 0:
                                        currentColor = Color.FromArgb(existingPixel.A, values[x * 64 + y], existingPixel.G, existingPixel.B);
                                        break;
                                    case 1:
                                        currentColor = Color.FromArgb(existingPixel.A, existingPixel.R, values[x * 64 + y], existingPixel.B);
                                        break;
                                    case 2:
                                        currentColor = Color.FromArgb(existingPixel.A, existingPixel.R, existingPixel.G, values[x * 64 + y]);
                                        break;
                                    case 3:
                                        currentColor = Color.FromArgb(values[x * 64 + y], existingPixel.R, existingPixel.G, existingPixel.B);
                                        break;
                                }
                                splatmaps[si].SetPixel(x + xOff, y + yOff, currentColor);
                            }
                        }

                        for (int x = 0; x < 64; x++)
                        {
                            for (int y = 0; y < 64; y++)
                            {
                                var currentColor = new Color();

                                currentColor = Color.FromArgb(255, values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);

                                textureLayers[adtfile.texChunks[c].layers[li].textureId].SetPixel(x + xOff, y + yOff, currentColor);
                            }
                        }


                    }
                }//Layer Loop

                //----------------------------------------------------------------------------------------------------------
                //Change the offset
                //----------------------------------------------------------------------------------------------------------
                if (yOff + 64 > 960)
                {
                    yOff = 0;
                    if (xOff + 64 <= 960)
                    {
                        xOff += 64;
                    }
                }
                else
                {
                    yOff += 64;
                }
                //----------------------------------------------------------------------------------------------------------
            }//Chunk loop


            foreach (Bitmap bmp in splatmaps)
            {
                //----------------------------------------------------------------------------------------------------------
                //Fix bmp orientation:
                //----------------------------------------------------------------------------------------------------------
                bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
                //----------------------------------------------------------------------------------------------------------

                //----------------------------------------------------------------------------------------------------------
                //Store the generated map in the array
                //----------------------------------------------------------------------------------------------------------
                AlphaLayers.Add(bmp);
                //----------------------------------------------------------------------------------------------------------
            }

            foreach (Bitmap bmp in textureLayers)
            {
                //----------------------------------------------------------------------------------------------------------
                //Fix bmp orientation:
                //----------------------------------------------------------------------------------------------------------
                bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
                //----------------------------------------------------------------------------------------------------------

                //----------------------------------------------------------------------------------------------------------
                //Store the generated map in the array
                //----------------------------------------------------------------------------------------------------------
                TextureLayers.Add(bmp);
                //----------------------------------------------------------------------------------------------------------
            }

            #endregion
            //----------------------------------------------------------------------------------------------------------


        }

        private int IntCeil(int value, int divisor)
        {
            return ((value / divisor) + (value % divisor == 0 ? 0 : 1));
        }


    }
}
