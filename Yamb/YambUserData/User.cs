using System.Runtime.Serialization;

namespace YambUserData
{
    [DataContract]
    public class User
    {
        [DataMember]
        private int id;

        [DataMember]
        private int highscore = 0;

        [DataMember]
        private float averagescore = 0;

        [DataMember]
        private List<int> scores = new List<int>();

        public int Id { 
            get { return id; } 
        }

        public int Highscore { 
            get { return highscore; } 
        }

        public float AverageScore { 
            get { return averagescore; } 
        }

        public User(int id)
        {
            this.id = id;
            this.highscore = 0;
            this.averagescore = 0;
        }

        public User(int id, int score)
        {
            this.id = id;
            this.highscore = score;
            this.averagescore = score;
            scores.Add(score);
        }

        public User AddScore(int score)
        {
            scores.Add(score);

            if (score > Highscore) highscore = score;

            int n = scores.Count;
            averagescore = (AverageScore * (n - 1) + score) / n;

            return this;
        }
    }
}
