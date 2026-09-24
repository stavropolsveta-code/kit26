using System;
using System.Collections.Generic;
using UnityEngine;
using EchoOfAncients.Core;

namespace EchoOfAncients.Unity
{
    public class BoardView : MonoBehaviour
    {
        public float Spacing = 1f;
        public int Width = 8;
        public int Height = 8;

        public Board Board { get; private set; }
        public FieldRules Rules = FieldRules.None();
        public event Action<TurnReport> TurnResolved;
        public event Action<GridPosition, GridPosition> SwapRequested;

        private readonly Dictionary<GridPosition, TileView> _tileViews = new Dictionary<GridPosition, TileView>();
        private GridPosition? _selected;

        public void Build(Board board)
        {
            Board = board;
            Width = board.Width;
            Height = board.Height;

            foreach (var kvp in _tileViews)
            {
                if (kvp.Value != null) Destroy(kvp.Value.gameObject);
            }
            _tileViews.Clear();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(transform, false);
                    var view = go.AddComponent<TileView>();
                    var pos = new GridPosition(x, y);
                    view.Init(pos, Board.Cells[x, y], Spacing);
                    _tileViews[pos] = view;
                }
            }
        }

        public void Refresh()
        {
            foreach (var kvp in _tileViews)
            {
                kvp.Value.Refresh(Spacing);
            }
        }

        public void HandleClick(GridPosition position)
        {
            if (Board == null) return;

            if (_selected.HasValue && _selected.Value.Equals(position))
            {
                _selected = null;
                return;
            }

            if (_selected.HasValue)
            {
                var from = _selected.Value;
                _selected = null;

                if (IsAdjacent(from, position))
                {
                    SwapRequested?.Invoke(from, position);
                }
                else
                {
                    _selected = position;
                }
            }
            else
            {
                _selected = position;
            }
        }

        public void ApplyMove(GridPosition from, GridPosition to)
        {
            if (!BoardResolver.TryMove(Board, from, to, out var report, Rules))
            {
                Refresh();
                return;
            }

            Refresh();
            TurnResolved?.Invoke(report);
        }

        private static bool IsAdjacent(GridPosition a, GridPosition b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) == 1;
        }

        private void Update()
        {
            if (Board == null) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = Camera.main != null ? Camera.main.ScreenPointToRay(Input.mousePosition) : default;
            if (Camera.main != null && Physics.Raycast(ray, out var hit))
            {
                var view = hit.collider.GetComponentInParent<TileView>();
                if (view != null)
                {
                    HandleClick(view.Position);
                }
            }
        }
    }
}