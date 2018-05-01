using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public class SeatExpressionHandler
    {
        private string _expression;

        private string GetLastSeat()
        {
            int? id = null;
            foreach (var s in _expression.Split(','))
            {
                int index = s.IndexOf("-");
                if (index >= 0)
                {
                    int start = Convert.ToInt32(s.Substring(0, index));
                    int end = Convert.ToInt32(s.Substring(index + 1));
                    if (id == null || end > id)
                        id = start;
                }
                else
                {
                    int v = Convert.ToInt32(s);
                    if (id == null || v > id)
                        id = v;

                }
            }

            return id?.ToString();
        }

        private string GetFirstSeat()
        {
            int? id = null;
            foreach (var s in _expression.Split(','))
            {
                int index = s.IndexOf("-");
                if (index >= 0)
                {
                    int start = Convert.ToInt32(s.Substring(0, index));
                    int end = Convert.ToInt32(s.Substring(index + 1));
                    if (id == null || start < id)
                        id = start;
                }
                else
                {
                    int v = Convert.ToInt32(s);
                    if (id == null || v < id)
                        id = v;

                }
            }

            return id?.ToString();
        }

        private int GetTotalSeats()
        {
            int total = 0;
            foreach (var s in _expression.Split(','))
            {
                int index = s.IndexOf("-");
                if (index >= 0)
                {
                    int start = Convert.ToInt32(s.Substring(0, index));
                    int end = Convert.ToInt32(s.Substring(index + 1));
                    total += (end - start);
                }
                else
                {
                    total++;
                }
            }

            return total;
        }

        private bool HasSeat(string id)
        {
            int v = Convert.ToInt32(id);
            foreach (var s in _expression.Split(','))
            {
                int index = s.IndexOf("-");
                if (index >= 0)
                {
                    int start = Convert.ToInt32(s.Substring(0, index));
                    int end = Convert.ToInt32(s.Substring(index + 1));
                    if (v >= start && v <= end)
                        return true;
                }
                else
                {
                    if (s == id)
                        return true;
                }
            }

            return false;
        }

        private void AppendExpr(string expr)
        {
            if (_expression == string.Empty)
                _expression = expr;
            else
                _expression = $"{_expression},{expr}";
        }

        public SeatExpressionHandler()
        {
            _expression = string.Empty;
        }

        public SeatExpressionHandler(string expr)
        {
            this._expression = expr;
        }

        public bool Contains(string seatId)
        {
            return HasSeat(seatId);
        }

        public void AddSeat(string id)
        {
            if (!HasSeat(id))
            {
                AppendExpr(id);
            }
        }

        public void AddRange(string start, string end)
        {
            if (!HasSeat(start) && !HasSeat(end))
                AppendExpr($"{start}-{end}");
        }

        public void RemoveSeat(string id)
        {
            if (HasSeat(id))
            {

            }
        }

        public void RemoveRange(string start, string end)
        {

        }

        public int TotalSeats => GetTotalSeats();

        public override string ToString()
        {
            return _expression;
        }
    }
}