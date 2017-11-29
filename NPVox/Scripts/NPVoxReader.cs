using UnityEngine;
using System.IO;

/// Based on implementation by Giawa https://www.giawa.com/magicavoxel-c-importer/
public class NPVoxReader
{
    // this is the default palette of voxel colors (the RGBA chunk is only included if the palette is differe)
    // custom voxel format by Giava (16 bits, 0RRR RRGG GGGB BBBB)
    // (ushort)(((r & 0x1f) << 10) | ((g & 0x1f) << 5) | (b & 0x1f));
    private static ushort[] DEFAULT_COLORS = new ushort[] { 0, 32767, 25599, 19455, 13311, 7167, 1023, 32543, 25375, 19231, 13087, 6943, 799, 32351, 25183,
        19039, 12895, 6751, 607, 32159, 24991, 18847, 12703, 6559, 415, 31967, 24799, 18655, 12511, 6367, 223, 31775, 24607, 18463, 12319, 6175, 31,
        32760, 25592, 19448, 13304, 7160, 1016, 32536, 25368, 19224, 13080, 6936, 792, 32344, 25176, 19032, 12888, 6744, 600, 32152, 24984, 18840,
        12696, 6552, 408, 31960, 24792, 18648, 12504, 6360, 216, 31768, 24600, 18456, 12312, 6168, 24, 32754, 25586, 19442, 13298, 7154, 1010, 32530,
        25362, 19218, 13074, 6930, 786, 32338, 25170, 19026, 12882, 6738, 594, 32146, 24978, 18834, 12690, 6546, 402, 31954, 24786, 18642, 12498, 6354,
        210, 31762, 24594, 18450, 12306, 6162, 18, 32748, 25580, 19436, 13292, 7148, 1004, 32524, 25356, 19212, 13068, 6924, 780, 32332, 25164, 19020,
        12876, 6732, 588, 32140, 24972, 18828, 12684, 6540, 396, 31948, 24780, 18636, 12492, 6348, 204, 31756, 24588, 18444, 12300, 6156, 12, 32742,
        25574, 19430, 13286, 7142, 998, 32518, 25350, 19206, 13062, 6918, 774, 32326, 25158, 19014, 12870, 6726, 582, 32134, 24966, 18822, 12678, 6534,
        390, 31942, 24774, 18630, 12486, 6342, 198, 31750, 24582, 18438, 12294, 6150, 6, 32736, 25568, 19424, 13280, 7136, 992, 32512, 25344, 19200,
        13056, 6912, 768, 32320, 25152, 19008, 12864, 6720, 576, 32128, 24960, 18816, 12672, 6528, 384, 31936, 24768, 18624, 12480, 6336, 192, 31744,
        24576, 18432, 12288, 6144, 28, 26, 22, 20, 16, 14, 10, 8, 4, 2, 896, 832, 704, 640, 512, 448, 320, 256, 128, 64, 28672, 26624, 22528, 20480,
        16384, 14336, 10240, 8192, 4096, 2048, 29596, 27482, 23254, 21140, 16912, 14798, 10570, 8456, 4228, 2114  };

    // this will contain the converted colors from the default palette 
    private static Color32[] DEFAULT_VOX_COLORS;

    static NPVoxReader()
    {
        DEFAULT_VOX_COLORS = new Color32[256];
        for (int i = 0; i < DEFAULT_COLORS.Length; i++)
        {
            // convert back:
            Color32 targetColor = new Color32();
            ushort sourceColor = DEFAULT_COLORS[i];
            targetColor.r = (byte)((sourceColor >> 10) << 3);
            targetColor.g = (byte)(((sourceColor >> 5) & 0x1f) << 3);
            targetColor.b = (byte)((sourceColor & 0x1f) << 3);
            targetColor.a = 255;
            DEFAULT_VOX_COLORS[i] = targetColor;
        }
    }

    public static NPVoxModel Read(BinaryReader stream, NPVoxModel reuse = null)
    {
        NPVoxModel voxModel = null;
        Color32[] colors = null;

        // check out http://voxel.codeplex.com/wikipage?title=VOX%20Format&referringTitle=Home for the file format used below
        string magic = new string(stream.ReadChars(4));
        stream.ReadInt32(); // version

        // a MagicaVoxel .vox file starts with a 'magic' 4 character 'VOX ' identifier
        if (magic == "VOX ")
        {
            while (stream.BaseStream.Position < stream.BaseStream.Length)
            {
                // each chunk has an ID, size and child chunks
                char[] chunkId = stream.ReadChars(4);
                int chunkSize = stream.ReadInt32();
                stream.ReadInt32(); // childChunks
                string chunkName = new string(chunkId);

                // there are only 2 chunks we only care about, and they are SIZE and XYZI
                if (chunkName == "SIZE")
                {
                    sbyte x = (sbyte)stream.ReadInt32();
                    sbyte y = (sbyte)stream.ReadInt32();
                    sbyte z = (sbyte)stream.ReadInt32();
                    voxModel = (NPVoxModel)NPVoxModel.NewInstance(new NPVoxCoord(x, y, z), reuse);
                    stream.ReadBytes(chunkSize - 4 * 3); // ???
                }
                else if (chunkName == "XYZI")
                {
                    // XYZI contains n voxels
                    int numVoxels = stream.ReadInt32();
                    voxModel.NumVoxels = numVoxels;
                    for (int i = 0; i < numVoxels; i++)
                        voxModel.SetVoxel(new NPVoxCoord(stream.ReadSByte(), stream.ReadSByte(), stream.ReadSByte()), stream.ReadByte());
                }
                else if (chunkName == "RGBA")
                {
                    colors = new Color32[256];
                    colors[0] = new Color32(0, 0, 0, 0);

                    for (int i = 1; i < 256; i++)
                    {
                        byte r = stream.ReadByte();
                        byte g = stream.ReadByte();
                        byte b = stream.ReadByte();
                        byte a = stream.ReadByte();
                        colors[i] = new Color32(r, g, b, a);
                    }
                    stream.ReadBytes(4); // read the last color 256 which is not used
                }
                else stream.ReadBytes(chunkSize);   // read any excess bytes
            }

            voxModel.Colortable = colors != null ? colors : DEFAULT_VOX_COLORS;
        }

        return voxModel;
    }
}
