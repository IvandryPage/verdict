using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class TestimonyData
    {
        [SerializeField] private string id;

        [SerializeField] private string title;

        [TextArea(2, 4)]
        [SerializeField] private string description;

        [SerializeField] private List<StatementData> statements = new();

        public string Id => id;

        public string Title => title;

        public string Description => description;

        public IReadOnlyList<StatementData> Statements => statements;

        public TestimonyData() { }

        public TestimonyData(string id, string title, string description)
        {
            this.id = id;
            this.title = title;
            this.description = description;
        }

        public void AddStatement(StatementData statement)
        {
            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            statements.Add(statement);
        }

        public bool RemoveStatement(StatementData statement)
        {
            if (statement == null)
                return false;

            return statements.Remove(statement);
        }

        public void InsertStatement(int index, StatementData statement)
        {
            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            statements.Insert(index, statement);
        }

        public int IndexOf(StatementData statement)
        {
            if (statement == null)
                return -1;

            return statements.IndexOf(statement);
        }

        public void MoveStatement(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            StatementData statement = statements[oldIndex];

            statements.RemoveAt(oldIndex);

            statements.Insert(newIndex, statement);
        }

        public void ClearStatements()
        {
            statements.Clear();
        }

        public bool ContainsStatement(StatementData statement)
        {
            return statements.Contains(statement);
        }
    }
}
