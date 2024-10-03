using System.ComponentModel;
using System.Runtime.Serialization;

namespace YambSheetData
{
    [DataContract]
    public class Sheet
    {
        [DataMember]
        int[] rowDown = { -1, -1, -1, -1, -1, -1 };

        [DataMember]
        int[] rowUp = { -1, -1, -1, -1, -1, -1 };

        [DataMember]
        int moves = 0;

        public bool Complete
        {
            get { return moves == rowDown.Length + rowUp.Length; }
        }
        public bool InUse
        {
            get { return moves > 0; }
        }

        private int[] GetAvailableMoves()
        {
            int[] available = new int[2] { -1, -1 };

            for (int i = 0; i < rowDown.Length; i++)
            {
                if (rowDown[i] != -1) continue;
                available[0] = i + 1;
                break;
            }
            for (int i = rowUp.Length - 1; i >= 0; i--)
            {
                if (rowUp[i] != -1) continue;
                available[1] = i + 1;
                break;
            }
            return available;
        }

        public Sheet WriteMove(int[][] dice)
        {
            if (Complete) return this;

            int[] availableMoves = GetAvailableMoves(); //[0] - available move in rowDown, [1] - in rowUp; -1 if there is no available move
            bool upOrDown = true;                       //true - writing to rowDown, false - writing to rowUp

            if (availableMoves[0] == -1) upOrDown = false;
            if (availableMoves[1] == -1) upOrDown = true;

            //condition returns true if both are positive or negative
            //effectively, here they can't both be negative since it is ensured that at least one move is available with the if statement in the beggining of this function
            if (availableMoves[0] * availableMoves[1] > 0)
            {
                int downCnt = 0, upCnt = 0;
                int[] firstThrow = dice[0];

                for (int i = 0; i < firstThrow.Length; i++)
                {
                    if (firstThrow[i] == availableMoves[0]) downCnt++;
                    if (firstThrow[i] == availableMoves[1]) upCnt++;
                }

                upOrDown = downCnt >= upCnt;
            }

            int target = upOrDown ? availableMoves[0] : availableMoves[1];
            int diceSaved = 0;

            for (int iHand = 0; iHand < dice.Length; iHand++)
            {
                int[] hand = dice[iHand];
                for (int i = diceSaved; i < hand.Length; i++)
                {
                    if (hand[i] == target) diceSaved++;
                }
            }

            diceSaved = int.Min(5, diceSaved);

            if (upOrDown)
            {
                rowDown[availableMoves[0] - 1] = diceSaved * target;
            }
            else
            {
                rowUp[availableMoves[1] - 1] = diceSaved * target;
            }

            moves++;

            return this;
        }

        public Sheet WriteMoveToField(int cnt, int target, string where)
        {
            int[] availableMoves = GetAvailableMoves();

            if (!(availableMoves[0] == target && where == "down")
                && !(availableMoves[1] == target && where == "up")) return this;

            if (where == "down") rowDown[target - 1] = cnt * target;
            if (where == "up") rowUp[target - 1] = cnt * target;
            moves++;

            return this;
        }

        public int GetScore()
        {
            int sumDown = 0, sumUp = 0;

            for (int i = 0; i < rowDown.Length; i++)
            {
                sumDown += rowDown[i];
                sumUp += rowUp[i];
            }
            if (sumDown >= 60) sumDown += 30;
            if (sumUp >= 60) sumUp += 30;

            return sumDown + sumUp;
        }

        public int[][] SerializationArray()
        {
            int[][] ret = new int[2][];
            ret[0] = new int[6];
            ret[1] = new int[6];

            for (int i = 0; i < rowDown.Length; i++) ret[0][i] = rowDown[i];
            for (int i = 0; i < rowUp.Length; i++) ret[1][i] = rowUp[i];
            return ret;
        }

        public Sheet Clear()
        {
            moves = 0;
            for(int i = 0; i < 6; i++)
            {
                rowDown[i] = -1;
                rowUp[i] = -1;
            }
            return this;
        }
    }
}
