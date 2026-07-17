using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Service;

namespace Verdict.Editor.CaseFlow.Inspector
{
    public sealed class WitnessCard : VisualElement
    {
        private readonly EditorSession session;

        private readonly CaseEditService editService;

        private readonly WitnessContext context;


        public WitnessCard(
            EditorSession session,
            CaseEditService editService,
            WitnessContext context)
        {
            this.session = session;
            this.editService = editService;
            this.context = context;


            style.marginBottom = 10;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;


            Build();
        }



        private void Build()
        {
            Button header =
                new(() =>
                {
                    session.Selection.SelectWitness(context);
                });


            header.text =
                $"Witness : {context.Witness.Id}";


            header.style.unityFontStyleAndWeight =
                FontStyle.Bold;


            Add(header);



            Foldout foldout = new()
            {
                text =
                    $"Testimonies ({context.Witness.Testimonies.Count})",

                value = true
            };



            foreach(TestimonyData testimony
                in context.Witness.Testimonies)
            {
                TestimonyContext testimonyContext =
                    session.CreateTestimonyContext(
                        context.Witness,
                        testimony);


                foldout.Add(
                    new TestimonyCard(
                        session,
                        editService,
                        testimonyContext));
            }



            Button add =
                new(() =>
                {
                    editService.CreateTestimony(
                        context.Witness);
                })
                {
                    text = "+ Add Testimony"
                };


            foldout.Add(add);


            Add(foldout);
        }
    }
}
