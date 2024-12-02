#if UNITY_EDITOR
using System;
using UnityEngine;

using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

using UnityEditor.AssetImporters;
using UnityEditor;

namespace KWS
{
    [ScriptedImporter(1, "kwsTexture")]
    internal class TextureImporter : ScriptedImporter
    {
       
        public static TextureFormat GetCompatibleTextureFormat(BuildTarget target, KW_Extensions.UsedChannels usedChannels, bool isHDR)
        {
            if (isHDR)
            {
                return usedChannels switch
                {
                    KW_Extensions.UsedChannels._R    => (IsMobileTarget(target) ? TextureFormat.ASTC_HDR_8x8 : TextureFormat.BC6H),
                    KW_Extensions.UsedChannels._RG   => (IsMobileTarget(target) ? TextureFormat.ASTC_HDR_8x8 : TextureFormat.BC6H),
                    KW_Extensions.UsedChannels._RGB  => (IsMobileTarget(target) ? TextureFormat.ASTC_HDR_8x8 : TextureFormat.BC6H),
                    KW_Extensions.UsedChannels._RGBA => (IsMobileTarget(target) ? TextureFormat.ASTC_HDR_8x8 : TextureFormat.RGBAHalf),
                    _                  => throw new ArgumentOutOfRangeException(nameof(usedChannels), usedChannels, null)
                };
            }
            else
            {
                return usedChannels switch
                {
                    KW_Extensions.UsedChannels._R    => (IsMobileTarget(target) ? TextureFormat.EAC_R : TextureFormat.BC4),
                    KW_Extensions.UsedChannels._RG   => (IsMobileTarget(target) ? TextureFormat.EAC_RG : TextureFormat.BC5),
                    KW_Extensions.UsedChannels._RGB  => (IsMobileTarget(target) ? TextureFormat.ASTC_8x8 : TextureFormat.BC7),
                    KW_Extensions.UsedChannels._RGBA => (IsMobileTarget(target) ? TextureFormat.ASTC_8x8 : TextureFormat.BC7),
                    _                  => throw new ArgumentOutOfRangeException(nameof(usedChannels), usedChannels, null)
                };
            }
        }

        static bool IsMobileTarget(BuildTarget target)
        {
            if (target == BuildTarget.Android || target == BuildTarget.Switch || target == BuildTarget.iOS) return true;
            else return false;
        }

        public static void Write(Texture2D r, string path, bool useAutomaticCompressionFormat, KW_Extensions.UsedChannels usedChannels = default, bool isHDR = false, bool mipChain = false)
        {
            var fullName = path + ".kwsTexture";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullName));
            }

            using (FileStream fs = new FileStream(path + ".kwsTexture", FileMode.Create))
            using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
            using (var bw = new BinaryWriter(zipStream))
            {
                var bytes = r.GetRawTextureData();

                bw.Write(0);
                bw.Write(useAutomaticCompressionFormat);
                bw.Write((int)usedChannels);
                bw.Write(isHDR);
                bw.Write((int)r.format);
                bw.Write(r.width);
                bw.Write(r.height);
                bw.Write(mipChain);
                bw.Write((int)r.wrapMode);
                bw.Write((int)r.filterMode);
                bw.Write(bytes.Length);
                bw.Write(bytes);


                
            }

            AssetDatabase.SaveAssets();
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            using (var zipFile = File.Open(ctx.assetPath, FileMode.Open))
            using (GZipStream zipStream = new GZipStream(zipFile, CompressionMode.Decompress))
            using (var r = new BinaryReader(zipStream))
            {
                var version = r.ReadInt32();
                if (version != 0)
                {
                    Debug.LogError("Version mismatch in kwsTexture aseset");
                    return;
                }

                var useAutomaticCompressionFormat = r.ReadBoolean();
                var usedChannels = (KW_Extensions.UsedChannels)r.ReadInt32();
                var isHDR = r.ReadBoolean();
                var format = (TextureFormat)r.ReadInt32();
                var width = r.ReadInt32();
                var height = r.ReadInt32();
                var mipChain = r.ReadBoolean();
                var wrapMode = (TextureWrapMode)(r.ReadInt32());
                var filterMode = (FilterMode)(r.ReadInt32());
                int length = r.ReadInt32();
                var bytes = r.ReadBytes(length);

                var tex = new Texture2D(width, height, format, mipChain, true);
                tex.wrapMode   = wrapMode;
                tex.filterMode = filterMode;
                tex.LoadRawTextureData(bytes);
                tex.Apply();

                if (useAutomaticCompressionFormat)
                {
                    var targetFormat = GetCompatibleTextureFormat(ctx.selectedBuildTarget, usedChannels, isHDR);
                    EditorUtility.CompressTexture(tex, targetFormat, TextureCompressionQuality.Normal);
                }

                string waterID = new DirectoryInfo(ctx.assetPath).Parent.Name;
                ctx.AddObjectToAsset(waterID, tex);
                ctx.SetMainObject(tex);
            }
        }
    }


    [ScriptedImporter(1, "kwsComputeBuffer")]
    internal class ComputeBufferImporter : ScriptedImporter
    {
        public enum ComputeBufferFormatEnum
        {
            FormatUint = 0,
            FormatUint2 = 1,
            FormatUint3 = 2,
            FormatUint4 = 3,
        }

        KWS_CoreUtils.Uint2[] PackToUint2(uint[] data)
        {
            var dataCountOffset = new KWS_CoreUtils.Uint2[data.Length / 2];
            var offset = 0;
            for (int i = 0; i < dataCountOffset.Length; i++)
            {
                dataCountOffset[i] = new KWS_CoreUtils.Uint2() { X = data[offset], Y = data[offset + 1] };
                offset += 2;
            }

            return dataCountOffset;
        }

        KWS_CoreUtils.Uint3[] PackToUint3(uint[] data)
        {
            var dataCountOffset = new KWS_CoreUtils.Uint3[data.Length / 4];
            var offset = 0;
            for (int i = 0; i < dataCountOffset.Length; i++)
            {
                dataCountOffset[i] = new KWS_CoreUtils.Uint3() { X = data[offset], Y = data[offset + 1], Z = data[offset + 2] };
                offset += 3;
            }

            return dataCountOffset;
        }

        KWS_CoreUtils.Uint4[] PackToUint4(uint[] data)
        {
            var dataCountOffset = new KWS_CoreUtils.Uint4[data.Length / 4];
            var offset = 0;
            for (int i = 0; i < dataCountOffset.Length; i++)
            {
                dataCountOffset[i] = new KWS_CoreUtils.Uint4() { X = data[offset], Y = data[offset + 1], Z = data[offset + 2], W = data[offset + 3] };
                offset += 4;
            }

            return dataCountOffset;
        }

        public static void Write<T>(T[] data, string path, ComputeBufferFormatEnum format)
        {
            using (FileStream fs = new FileStream(path + ".kwsComputeBuffer", FileMode.Create))
            using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
            using (var r = new BinaryWriter(zipStream))
            {
                var bytes = new byte[data.Length * Marshal.SizeOf(default(T))];
                Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

                r.Write((int)format);
                r.Write(bytes.Length);
                r.Write(bytes);
            }
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            using (var zipFile = File.Open(ctx.assetPath, FileMode.Open))
            using (GZipStream zipStream = new GZipStream(zipFile, CompressionMode.Decompress))
            using (var r = new BinaryReader(zipStream))
            {
                var format = (ComputeBufferFormatEnum)r.ReadInt32();
                var length = r.ReadInt32();
                var bytes = r.ReadBytes(length);
                var dataRaw = new uint[bytes.Length / Marshal.SizeOf(default(uint))];
                Buffer.BlockCopy(bytes, 0, dataRaw, 0, bytes.Length);

                var bufferGO = new GameObject("ComputeBufferObject");
                var computeBufferObject = bufferGO.AddComponent<ComputeBufferObject>();
               
                switch (format)
                {
                    case ComputeBufferFormatEnum.FormatUint:
                        computeBufferObject.ComputeBufferDataUint1 = dataRaw;
                        computeBufferObject.PackedSize = 1;
                        break;
                    case ComputeBufferFormatEnum.FormatUint2:
                        computeBufferObject.ComputeBufferDataUint2 = PackToUint2(dataRaw);
                        computeBufferObject.PackedSize = 2;
                        break;
                    case ComputeBufferFormatEnum.FormatUint3:
                        computeBufferObject.ComputeBufferDataUint3 = PackToUint3(dataRaw);
                        computeBufferObject.PackedSize = 3;
                        break;
                    case ComputeBufferFormatEnum.FormatUint4:
                        computeBufferObject.ComputeBufferDataUint4 = PackToUint4(dataRaw);
                        computeBufferObject.PackedSize = 4;
                        break;
                }

                ctx.AddObjectToAsset(bufferGO.GetHashCode().ToString(), bufferGO);
                ctx.SetMainObject(bufferGO);
                //AssetDatabase.Refresh();
            }
        }
    }
}
#endif