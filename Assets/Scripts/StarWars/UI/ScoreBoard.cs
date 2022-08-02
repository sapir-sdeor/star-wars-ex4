using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Avrahamy;

namespace StarWars.UI {
    public class ScoreBoard : MonoBehaviour {
        [Serializable]
        public class ScorePanel {
            public Text nameText;
            public Text scoreText;
            public int Score {
                get {
                    return _score;
                }
                set {
                    _score = value;
                    scoreText.text = _score.ToString();
                }
            }
            private int _score = 0;
        }

        [SerializeField] ScorePanel[] _scores;

        private int index = 0;
        private readonly Dictionary<string, ScorePanel> scores =
            new Dictionary<string, ScorePanel>();

        protected void Awake() {
            index = 0;
            foreach (var score in _scores) {
                score.nameText.gameObject.SetActive(false);
            }
        }

        public void Add(string name) {
            DebugAssert.Assert(!string.IsNullOrEmpty(name), "No name given to ship. Did you override Awake without calling base.Awake()?");
            var score = _scores[index++];
            score.nameText.gameObject.SetActive(true);
            score.nameText.text = name;
            score.scoreText.text = "0";

            scores[name] = score;
        }

        public void AddScore(string name, int points) {
            DebugLog.Log(LogTag.Gameplay, $"{name} Scored {points}");
            scores[name].Score += points;
        }
    }
}
