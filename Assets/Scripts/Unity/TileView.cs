using UnityEngine;
using EchoOfAncients.Core;

namespace EchoOfAncients.Unity
{
    public static class TilePalette
    {
        public static Color GetColor(TileType type)
        {
            switch (type)
            {
                case TileType.Leaf: return new Color(0.33f, 0.78f, 0.35f);
                case TileType.Wood: return new Color(0.72f, 0.52f, 0.28f);
                case TileType.Stone: return new Color(0.55f, 0.58f, 0.62f);
                case TileType.Water: return new Color(0.30f, 0.55f, 0.92f);
                case TileType.Fire: return new Color(0.89f, 0.32f, 0.22f);
                case TileType.Crystal: return new Color(0.78f, 0.42f, 0.92f);
                default: return Color.clear;
            }
        }
    }

    public class TileView : MonoBehaviour
    {
        public GridPosition Position { get; private set; }

        private Cell _cell;
        private Renderer _chipRenderer;
        private Renderer _iceRenderer;
        private Renderer _rockRenderer;
        private Renderer _markerRenderer;

        public void Init(GridPosition position, Cell cell, float spacing)
        {
            Position = position;
            _cell = cell;
            name = $"Tile_{position.X}_{position.Y}";
            transform.localPosition = new Vector3(position.X * spacing, position.Y * spacing, 0f);
            Refresh(spacing);
        }

        public void Refresh(float spacing)
        {
            if (_chipRenderer == null)
            {
                _chipRenderer = CreatePrimitive("Chip", PrimitiveType.Cube, 0.9f * spacing);
            }

            bool hasChip = _cell.Chip != null && !_cell.Chip.IsEmpty;
            _chipRenderer.gameObject.SetActive(hasChip);
            if (hasChip)
            {
                _chipRenderer.material.color = TilePalette.GetColor(_cell.Chip.Type);
            }

            UpdateMarker(spacing);
            UpdateObstacles(spacing);
        }

        private void UpdateMarker(float spacing)
        {
            bool hasSpecial = _cell.Chip != null && _cell.Chip.IsSpecial;
            if (hasSpecial && _markerRenderer == null)
            {
                _markerRenderer = CreatePrimitive("Marker", PrimitiveType.Sphere, 0.35f * spacing);
            }
            if (_markerRenderer != null)
            {
                _markerRenderer.gameObject.SetActive(hasSpecial);
                if (hasSpecial)
                {
                    switch (_cell.Chip.Special)
                    {
                        case SpecialType.Bomb: _markerRenderer.material.color = Color.black; break;
                        case SpecialType.LineHorizontal: _markerRenderer.material.color = Color.white; break;
                        case SpecialType.LineVertical: _markerRenderer.material.color = Color.cyan; break;
                    }
                }
            }
        }

        private void UpdateObstacles(float spacing)
        {
            bool hasIce = _cell.Obstacle.Type == ObstacleType.Ice;
            bool hasRock = _cell.Obstacle.Type == ObstacleType.Rock;

            if (hasIce && _iceRenderer == null)
            {
                _iceRenderer = CreatePrimitive("Ice", PrimitiveType.Cube, 1.02f * spacing);
                _iceRenderer.material.color = new Color(0.75f, 0.92f, 1f, 0.55f);
            }
            if (_iceRenderer != null) _iceRenderer.gameObject.SetActive(hasIce);

            if (hasRock && _rockRenderer == null)
            {
                _rockRenderer = CreatePrimitive("Rock", PrimitiveType.Cube, 1.0f * spacing);
                _rockRenderer.material.color = new Color(0.25f, 0.24f, 0.22f);
            }
            if (_rockRenderer != null) _rockRenderer.gameObject.SetActive(hasRock);
        }

        private Renderer CreatePrimitive(string label, PrimitiveType type, float size)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = label;
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * size;
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            return renderer;
        }
    }
}