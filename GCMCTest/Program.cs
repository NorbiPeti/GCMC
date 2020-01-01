using System;
using System.Threading.Channels;
using fNbt;
using GCMC;

namespace GCMCTest
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var nbtFile = new NbtFile();
            nbtFile.LoadFromFile("r.-1.-1.mca", NbtCompression.None, tag =>
            {
                Console.WriteLine(tag);
                return true;
            });
            Console.WriteLine(nbtFile);*/
            using (var rf = new RegionFile("r.-1.-1.mca"))
            {
                Console.WriteLine(rf);
                Console.WriteLine(rf.HasChunk(1, 2));
                Console.WriteLine(rf.LastModified + " " + rf.SizeDelta);
                foreach (var br in rf.GetChunks())
                {
                    var nbt = new NbtFile();
                    nbt.LoadFromStream(br.BaseStream, NbtCompression.AutoDetect);
                    Console.WriteLine(nbt);
                    Console.WriteLine(nbt.RootTag);
                }
            }
        }
    }
}
