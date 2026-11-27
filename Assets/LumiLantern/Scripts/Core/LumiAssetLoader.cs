using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LumiLantern
{
    public static class LumiAssetLoader
    {
        private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite GetSprite(string name)
        {
            if (cache.TryGetValue(name, out var cached) && cached != null)
            {
                return cached;
            }

            // 1. Try Resources.Load
            Sprite resSprite = Resources.Load<Sprite>("LumiSprites/" + name);
            if (resSprite != null)
            {
                cache[name] = resSprite;
                return resSprite;
            }

            // 2. Fallback: Direct file load from Assets/LumiLantern/Sprites
            string[] possiblePaths = new string[]
            {
                Path.Combine(Application.dataPath, "LumiLantern", "Sprites", name + ".png"),
                Path.Combine(Application.dataPath, "Resources", "LumiSprites", name + ".png")
            };

            foreach (string filePath in possiblePaths)
            {
                if (File.Exists(filePath))
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        Vector4 border = Vector4.zero;
                        if (name.StartsWith("btn_") || name.StartsWith("card_") || name.StartsWith("tile_"))
                        {
                            border = new Vector4(24, 24, 24, 24);
                        }

                        Sprite fileSprite = Sprite.Create(
                            tex,
                            new Rect(0, 0, tex.width, tex.height),
                            new Vector2(0.5f, 0.5f),
                            100f,
                            0,
                            SpriteMeshType.FullRect,
                            border
                        );
                        fileSprite.name = name;
                        cache[name] = fileSprite;
                        return fileSprite;
                    }
                }
            }

            Debug.LogWarning($"[LumiAssetLoader] Sprite not found: {name}");
            return null;
        }
    }
}
