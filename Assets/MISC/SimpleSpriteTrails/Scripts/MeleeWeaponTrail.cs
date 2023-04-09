using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SimpleSpriteTrails.Scripts
{
    /// <summary>
    /// This script creates weapon trails as second sprite. Unlike similar assets, resulting trail's form repeats weapon sprite edge.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MeleeWeaponTrail : MonoBehaviour
    {
        public SpriteRenderer WeaponRenderer;
        public TrailDirection Direction;
        [Range(0, 1000)]
        public float TrailLength = 100;
        [Range(0, 1)]
        public float TrailBend = 0.25f;
        public AnimationCurve TrailCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
        [Tooltip("Warning: slows down the performance if checked.")]
        public bool RemoveSpaces;
        public bool Disabled;

        /// <summary>
        /// Build a trail
        /// </summary>
        public void Build()
        {
            if (Disabled || WeaponRenderer.sprite == null) return;

            var texture = CopyNotReadableSprite(WeaponRenderer.sprite);
            var trail = new Texture2D(texture.width, texture.height);

            ClearTexture(trail);

            var pixels = CreateTrailLine(texture, trail);
            
            FadeTrailLine(pixels, trail);

            if (TrailBend > 0 && RemoveSpaces)
            {
                FillSpaces(trail);
            }

            trail.Apply();

            var pivot = new Vector2(WeaponRenderer.sprite.pivot.x / WeaponRenderer.sprite.rect.width, WeaponRenderer.sprite.pivot.y / WeaponRenderer.sprite.rect.height);

            GetComponent<SpriteRenderer>().sprite = Sprite.Create(trail, new Rect(0, 0, texture.width, texture.height), pivot, 100);
        }

        private Texture2D CopyNotReadableSprite(Sprite sprite)
        {
            var buffer = new Texture2D(sprite.texture.width, sprite.texture.height);
            var renderTexture = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(sprite.texture, renderTexture);
            RenderTexture.active = renderTexture;
            buffer.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            buffer.Apply();

            var texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
            var pixels = buffer.GetPixels((int) sprite.textureRect.x, (int) sprite.textureRect.y, (int) sprite.textureRect.width, (int) sprite.textureRect.height);

            ClearTexture(texture);
            texture.SetPixels((int) sprite.textureRectOffset.x, (int) sprite.textureRectOffset.y, (int) sprite.textureRect.width, (int) sprite.textureRect.height, pixels);
            texture.Apply();

            return texture;
        }

        private List<Vector2> CreateTrailLine(Texture2D texture, Texture2D trail)
        {
            var line = new List<Vector2>();
            var color = Color.white;
            var pixels = texture.GetPixels();
            var width = texture.width;
            var height = texture.height;

            switch (Direction)
            {
                case TrailDirection.Left:
                    for (var y = 0; y < width; y++)
                    for (var x = 0; x < height; x++)
                    if (FindEdge(trail, pixels, x, y, width, color, line)) break;
                    break;
                case TrailDirection.Right:
                    for (var y = 0; y < height; y++)
                    for (var x = width - 1; x >= 0; x--)
                    if (FindEdge(trail, pixels, x, y, width, color, line)) break;
                    break;
                case TrailDirection.Up:
                    for (var x = 0; x < width; x++)
                    for (var y = height - 1; y >= 0; y--)
                    if (FindEdge(trail, pixels, x, y, width, color, line)) break;
                    break;
                case TrailDirection.Down:
                    for (var x = 0; x < width; x++)
                    for (var y = 0; y < height; y++)
                    if (FindEdge(trail, pixels, x, y, width, color, line)) break;
                    break;
            }

            return line;
        }

        private static bool FindEdge(Texture2D trail, Color[] pixels, int x, int y, int width, Color color, List<Vector2> line)
        {
            if (pixels[x + y * width].a > 0.5f)
            {
                trail.SetPixel(x, y, color);
                line.Add(new Vector2(x, y));
                return true;
            }

            return false;
        }

        private void FadeTrailLine(IList<Vector2> pixels, Texture2D trail)
        {
            var width = trail.width;
            var height = trail.height;
            var length = Direction == TrailDirection.Left || Direction == TrailDirection.Right ? pixels.Last().y - pixels.First().y : pixels.Last().x - pixels.First().x;

            foreach (var pixel in pixels)
            {
                var x = (int) pixel.x;
                var y = (int) pixel.y;
                var delta = Direction == TrailDirection.Left || Direction == TrailDirection.Right ? y - pixels[0].y : x - pixels[0].x;
                var iterations = TrailLength * TrailCurve.Evaluate(delta / length);
                var color = trail.GetPixel(x, y);

                for (var i = 1; i < iterations; i++)
                {
                    color.a = 1 - i / iterations;

                    switch (Direction)
                    {
                        case TrailDirection.Left:
                            if (x - i >= 0) trail.SetPixel(x - i, (int) (y * Mathf.Cos(i * Mathf.Deg2Rad * TrailBend)), color);
                            {
                                trail.SetPixel(x - i, (int) (y * Mathf.Cos(i * Mathf.Deg2Rad * TrailBend)), color);
                            }
                            break;
                        case TrailDirection.Right:
                            if (x + i < width)
                            {
                                trail.SetPixel(x + i, (int) (y * Mathf.Cos(i * Mathf.Deg2Rad * TrailBend)), color);
                            }
                            break;
                        case TrailDirection.Up:
                            if (y + i < height)
                            {
                                trail.SetPixel((int) (x * Mathf.Cos(i * Mathf.Deg2Rad * TrailBend)), y + i, color);
                            }
                            break;
                        case TrailDirection.Down:
                            if (y - i >= 0)
                            {
                                trail.SetPixel((int) (x * Mathf.Cos(i * Mathf.Deg2Rad * TrailBend)), y - i, color);
                            }
                            break;
                    }
                }
            }
        }

        private static void ClearTexture(Texture2D texture)
        {
            var pixels = new Color[texture.width * texture.height];
            var clear = Color.clear;

            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }

        private static void FillSpaces(Texture2D texture)
        {
            var pixels = texture.GetPixels();
            var width = texture.width;
            var height = texture.height;

            for (var y = 1; y < width - 1; y++)
            {
                for (var x = 1; x < height - 1; x++)
                {
                    if (pixels[x + y * width].a <= 0)
                    {
                        var above = pixels[x + (y + 1) * width];
                        var below = pixels[x + (y - 1) * width];

                        if (above != Color.clear && below != Color.clear)
                        {
                            texture.SetPixel(x, y, (above + below) / 2);
                        }
                    }
                }
            }
        }
    }
}