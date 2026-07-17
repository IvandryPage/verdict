using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
{
    public sealed class TestimonyCard : VisualElement
    {
        private readonly EditorSession session;

        private readonly CaseEditService editService;

        private readonly TestimonyContext context;



        public TestimonyCard(
            EditorSession session,
            CaseEditService editService,
            TestimonyContext context)
        {
            this.session = session;
            this.editService = editService;
            this.context = context;


            style.marginBottom = 8;
            style.marginLeft = 8;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 6;
            style.paddingBottom = 6;


            Build();
        }



        private void Build()
        {
            Button header =
                new(() =>
                {
                    session.Selection.SelectTestimony(
                        context);
                });


            header.text =
                $"Testimony : {context.Testimony.Title}";


            header.style.unityFontStyleAndWeight =
                FontStyle.Bold;


            Add(header);



            Foldout statements =
                new()
                {
                    text =
                        $"Statements ({context.Testimony.Statements.Count})",

                    value = true
                };



            foreach (StatementData statement
                in context.Testimony.Statements)
            {
                StatementContext statementContext =
                    session.CreateStatementContext(
                        context.Witness,
                        context.Testimony,
                        statement);



                Button statementButton =
                    new(() =>
                    {
                        session.Selection.SelectStatement(
                            statementContext);
                    });


                statementButton.text =
                    statement.Text;


                statements.Add(
                    statementButton);
            }



            Button add =
                new(() =>
                {
                    editService.CreateStatement(
                        context.Testimony);
                })
                {
                    text = "+ Add Statement"
                };


            statements.Add(add);


            Add(statements);
        }
    }
}
