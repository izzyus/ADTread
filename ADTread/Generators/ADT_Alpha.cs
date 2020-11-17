using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using WoWFormatLib.Structs.ADT;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Generators.ADT_Alpha
{
    class ADT_Alpha
    {
        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------
        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();
        public String SplatmapJSON;
        //-----------------------------------------------------------------------------------------------------------------

        public void GenerateAlphaMaps(ADT adtfile, int GenerationMode)
        {
            #region Splatmaps (1024x1024)
            if (GenerationMode == 6)
            {
                //----------------------------------------------------------------------------------------------------------
                //Generate the splatmap json
                //----------------------------------------------------------------------------------------------------------
                #region Splatmap JSON
                var materialJSON = "{\"chunkData\":{"; // New JSON file to save material data
                for (uint c = 0; c < adtfile.chunks.Count(); c++)
                {
                    materialJSON += "\"" + c + "\":[";
                    for (int li = 0; li < adtfile.texChunks[c].layers.Count(); li++)
                    {
                        if (adtfile.texChunks[c].alphaLayer != null)
                        {
                            var AlphaLayerName = adtfile.textures.filenames[adtfile.texChunks[c].layers[li].textureId].ToLower().Replace(".blp", "");
                            materialJSON += "{\"id\":\"" + AlphaLayerName.Replace("\\", "\\\\") + "\",\"scale\":\"" + 4 + "\"},"; //MAT.SCALE ISN'T SET, TODO: SEE WHY VALUES ARE MISSING
                        }
                    }
                    materialJSON = materialJSON.Substring(0, materialJSON.Length - 1); // remove tailing comma
                    materialJSON += "],"; // Close the subchunk array
                }
                materialJSON = materialJSON.Substring(0, materialJSON.Length - 1); // remove tailing comma
                var fullJSON = materialJSON + "},\"splatmapData\":{"; // create JSON data to include splatmap data
                materialJSON += "}}"; // Close the JSON data
                //Console.WriteLine(materialJSON);
                var matJSON = Newtonsoft.Json.Linq.JObject.Parse(materialJSON);
                //Console.WriteLine(matJSON);

                if (adtfile.textures.filenames.Length == 0)
                {
                    fullJSON += "\"id0\":\"null\",";
                }
                else
                {
                    for (int q = 0; q < adtfile.textures.filenames.Length; q++)
                    {
                        fullJSON += "\"id" + q + "\":\"" + adtfile.textures.filenames[q].Replace("\\", "\\\\").ToLower().Replace(".blp", "") + "\",";
                    }
                }
                fullJSON = fullJSON.Substring(0, fullJSON.Length - 1); // remove tailing comma
                fullJSON += "}}"; // Close the JSON data
                //Console.WriteLine(fullJSON);
                SplatmapJSON = fullJSON;
                var fullParsedJSON = Newtonsoft.Json.Linq.JObject.Parse(fullJSON);
                //Console.WriteLine(fullParsedJSON);
                #endregion
                //----------------------------------------------------------------------------------------------------------

                //----------------------------------------------------------------------------------------------------------
                //Generate the actual splatmaps
                //----------------------------------------------------------------------------------------------------------
                #region Splatmap Bitmap (Wrong)
                /*
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
                */
                #endregion
                #region Splatmap Bitmap

                var materialIDs = adtfile.textures.filenames;

                var imageCount = IntCeil(materialIDs.Length, 4);
                Console.WriteLine(string.Format("imageCount {0}", imageCount));

                int[][] pixelData = new int[imageCount][]; //A magic array of arrays
                for (var p = 0; p < pixelData.Length; p++)
                {
                    pixelData[p] = new int[1024 * 1024 * 4];
                    Console.WriteLine("PixelData Array: " + p);
                }

                // Writing a 1024x1024 image in 64x64 chunks
                var bytesPerPixel = 4;      // Each pixel has a R,G,B,A byte
                var bytesPerColumn = 262144; // A 'column' is 1024 pixels vertical (chunk) bytesPerRow * b
                var bytesPerRow = 4096;   // A 'row' is 1024 pixels horizontal (chunk) a * 4
                var bytesPerSubColumn = 16384;  // A 'subcolumn' is 64 pixels vertical (subchunk) bytesPerSubRow * b
                var bytesPerSubRow = 256;    // A 'subrow' is 64 pixels horizontal (subchunk) b * 4

                // Now before we draw each sub-chunk to TGA, we need to check it's texture list in json.
                // Based on what order the textures are for that sub-chunk, we may need to draw RGBA in a different order than 0,1,2,3
                // Loop Y first so we go left to right, top to bottom. Loop 16x16 subchunks to get the full chunk
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        int chunkIndex = (y * 16) + x;
                        var texChunk = adtfile.texChunks[chunkIndex];
                        var alphaLayers = texChunk.alphaLayer;// || [];
                        var textureLayers = texChunk.layers;
                        // If there is no texture data just skip it
                        if (textureLayers.Length > 0)
                        {
                            // If there is texture data, we need a base layer of red to flood the subchunk
                            for (var j = y * bytesPerColumn; j < (y * bytesPerColumn) + bytesPerColumn; j += bytesPerRow) // 1024 pixels wide, 64 pixels high = 65536 * 4 bytes = 262144 (looping y axis)
                            {
                                // Now we need to loop the x axis, 64 pixels long
                                for (var i = x * bytesPerSubRow; i < (x * bytesPerSubRow) + bytesPerSubRow; i += bytesPerPixel)// 64 pixels, 4 bytes each = 256
                                {
                                    var yloop = ((j / bytesPerRow) - (y * bytesPerColumn) / bytesPerRow);
                                    var xloop = ((i / 4) - ((x * bytesPerSubRow) / 4));
                                    var alphaIndex = (yloop * 64) + xloop;
                                    // The first TGA image will have base texture flooded with red.
                                    // THIS IS WRONG! Must flood base color, may be R, G, B, A
                                    // In the case of Azeroth_29_47 subchunk 2, base layer is blue
                                    if (pixelData[0][j + i + 0] == null) { Console.WriteLine("pixeldata[0]" + (j + i + 0) + " is undefined!"); }
                                    //pixelData[0][j + i + 0] = 255; // Red: (186865) NOT FLOODING RED ANYMORE

                                    //Read magic from json.... (guess who's comment is this)
                                    //var numberTextureLayers = matJSON.chunkData[chunkIndex].length;
                                    var numberTextureLayers = matJSON["chunkData"][chunkIndex.ToString()].Count();
                                    //Console.WriteLine(numberTextureLayers); //Don't ever do this again, i beg of you (again, guess)
                                    if (chunkIndex == 75 && xloop == 0 && yloop == 0) { Console.WriteLine("Chunk:" + chunkIndex + ", NumberTextureLayers: " + numberTextureLayers); }
                                    for (var k = 0; k < numberTextureLayers; k++)
                                    { // Looping texture layers, could be 0-100
                                      // k = 1, random materialID. This could be any RGBA, RGBA color!										
                                        if (matJSON["chunkData"][chunkIndex.ToString()][k].Count() == 0) { Console.WriteLine("Error: matJSON.chunkData[chunkIndex][k] is undefined"); } //No clue...
                                        var currentIndex = 0;
                                        var currentID = (string)matJSON["chunkData"][chunkIndex.ToString()][k]["id"]; //Probably not a good idea to use a string though
                                        //Console.WriteLine(currentID);
                                        //Console.WriteLine(currentID);//Why are you like this.... DO NOT DO THIS EVER A G  A I NNNNN
                                        //Console.WriteLine(String.Format("Material ids: {0}",materialIDs.Length));
                                        for (var l = 0; l < materialIDs.Length; l++)
                                        {
                                            //Console.WriteLine(string.Format("Current matid: {0}",materialIDs[l]));
                                            //Console.WriteLine(string.Format("Current index: {0}", currentIndex));
                                            //if (materialIDs[l] == currentID)
                                            if (adtfile.textures.filenames[l] == currentID)
                                            {
                                                currentIndex = l;
                                            }
                                        }
                                        if (currentIndex == -1) { Console.WriteLine("ERROR: Index is still -1 after loop:" + currentID); }
                                        var texIndex = currentIndex;
                                        if (chunkIndex == 75 && xloop == 0 && yloop == 0) { Console.WriteLine("texIndex: " + texIndex); }
                                        // alphaLayers is an array equal to length of textures and in the same order as materialIDs
                                        // each array item is an Array(64 * 64)
                                        // alphaLayers[0] is filled with red (255)

                                        // Red   / 0 has everything subtracted from it
                                        // Green / 1 has Blue & Alpha subtracted from it
                                        // Blue  / 2 has Alpha subtracted from it

                                        // Calculate image index, 1 TGA image for each 4 textures. index 0 includes base texture on channel 0
                                        var imageIndex = IntFloor(texIndex, 4);
                                        if (chunkIndex == 75 && xloop == 0 && yloop == 0) { Console.WriteLine("imageIndex: " + imageIndex); }

                                        // 0-3 RGBA. If imageIndex=0 this should not be 0 because that is basetexture
                                        var channelIndex = texIndex % 4;
                                        if (chunkIndex == 75 && xloop == 0 && yloop == 0) { Console.WriteLine("channelIndex: " + channelIndex); }

                                        // array  whichTGA   Pixel|chanel
                                        //if (pixelData[imageIndex] == null) { Console.WriteLine("pixelData[" + imageIndex + "] is undefined"); }
                                        //if (pixelData[imageIndex][j + i + channelIndex] == null) { Console.WriteLine("pixeldata[" + imageIndex + "]" + ", channelIndex:" + channelIndex + " is undefined!"); }

                                        // Write the actual pixel data
                                        if (k == 0)
                                        { // BASE LAYER 
                                            //Console.WriteLine(j + i + channelIndex);
                                            pixelData[imageIndex][j + i + channelIndex] = 255; // Flood Base Layer
                                        }
                                        else
                                        {
                                            if (alphaLayers[k].layer == null) //Added .layer, idk
                                            {
                                                Console.WriteLine("alphaLayers[k] is undefined: " + texIndex + ". Alphalayers length: " + alphaLayers.Length + ", Chunk: " + chunkIndex);
                                            }
                                            if (alphaLayers[k].layer[alphaIndex] == null)
                                            {
                                                Console.WriteLine("alphaLayers[" + k + "] alphaIndex[" + alphaIndex + "] is undefined!");
                                            }

                                            pixelData[imageIndex][j + i + channelIndex] = alphaLayers[k].layer[alphaIndex];

                                            var subtractImages = imageIndex % 4; // subtract all 4 channels (full image)

                                            for (var m = 0; m < imageCount; m++)
                                            { // All images
                                                if (chunkIndex == 75 && xloop == 0 && yloop == 0) { Console.WriteLine("m loop: " + m); }
                                                //log.write("pixelData[" + m + "] looping");
                                                if (pixelData[m] == null) { Console.WriteLine("ERROR: pixeldata[" + m + "] is undefined!"); }
                                                if (m != imageIndex)
                                                {
                                                    if (chunkIndex == 75 && xloop == 0 && yloop == 0) { Console.WriteLine("subtract full image: " + m); }
                                                    if (pixelData[m][j + i + 0] == null) { Console.WriteLine("pixelData[" + m + "] is undefined!"); }
                                                    pixelData[m][j + i + 0] -= alphaLayers[k].layer[alphaIndex];
                                                    pixelData[m][j + i + 1] -= alphaLayers[k].layer[alphaIndex];
                                                    pixelData[m][j + i + 2] -= alphaLayers[k].layer[alphaIndex];
                                                    pixelData[m][j + i + 3] -= alphaLayers[k].layer[alphaIndex];
                                                }
                                            }

                                            for (var n = 0; n < 4; n++)
                                            { // Loop 4 times
                                                if (n != channelIndex)
                                                {
                                                    pixelData[imageIndex][j + i + n] -= alphaLayers[k].layer[alphaIndex];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < imageCount; i++)
                {
                    var bmp = new Bitmap(1024, 1024);

                    for (int x = 0; x < 1024; x++)
                    {
                        for (int y = 0; y < 1024; y++)
                        {
                            Color currentColor = Color.FromArgb(ZeroClamp(pixelData[i][3]), ZeroClamp(pixelData[i][0]), ZeroClamp(pixelData[i][1]), ZeroClamp(pixelData[i][2]));
                            bmp.SetPixel(x, y, currentColor);
                        }
                    }
                    AlphaLayers.Add(bmp);
                }
                #endregion
                //----------------------------------------------------------------------------------------------------------
            }
            #endregion
        }

        private int IntCeil(int value, int divisor)
        {
            return ((value / divisor) + (value % divisor == 0 ? 0 : 1));
        }
        private int IntFloor(int value, int divisor)
        {
            return (value / divisor);
        }

        private int ZeroClamp(int x)
        {
            if (x < 0)
                return 0;

            return x;
        }
    }
}