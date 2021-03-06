using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ActivityUnitTests;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Globalization;
using Dev2.Common;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DotNetAggregateCalculateActivityTest : BaseActivityUnitTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DsfCalculateActivity_OnExecute")]
        public void DotNetAggregateCalculateActivity_OnExecute_GetCurrentDateTime_ResultContainsMilliseconds()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = @"now()", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><result></result></ADL>";
            TestData = "<root><ADL><result></result></ADL></root>";
            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "result", out string entry, out string error);

            // remove test datalist ;)

            var res = DateTime.Parse(entry,CultureInfo.InvariantCulture);

            if (res.Second == 0)
            {
                Thread.Sleep(10);
                result = ExecuteProcess();
                GetScalarValueFromEnvironment(result.Environment, "result", out entry, out error);
                res = DateTime.Parse(entry);
                Assert.IsTrue(res.Millisecond != 0);
            }
            Assert.IsTrue(res.Millisecond != 0);
        }

        [TestMethod]
        public void CalculateActivity_ValidFunction_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = @"Sum([[scalar]], 10)", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = "<root><ADL><RecordSet><Field>10</Field></RecordSet><RecordSet><Field>20</Field></RecordSet><scalar>2</scalar><result></result></ADL></root>";
            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "result", out string entry, out string error);

            // remove test datalist ;)

            Assert.AreEqual(entry, "12");

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfAggregateCalculateActivity_GetOutputs")]
        public void DsfAggregateCalculateActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfDotNetAggregateCalculateActivity { Expression = @"Sum([[scalar]], 10)", Result = "[[result]]" };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[result]]", outputs[0]);
        }

        [TestMethod]
        public void CalculateActivity_SimpleFunctionHandling_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = @"sum(10,20)", Result = "[[scalar]]" }
            };

            TestData = @"<ADL><scalar></scalar></ADL>";
            //TestData = ActivityStrings.CalculateActivityDataList;
            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "scalar", out string entry, out string error);

            // remove test datalist ;)

            Assert.AreEqual(entry, "30");
        }

        // SN - 07-09-2012 - Commented out until intellisense issue is patched up
        [TestMethod]
        public void CalculateActivity_CommaSeperatedArgs_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = @"Sum([[scalar]],[[RecordSet(1).Field]],[[RecordSet(2).Field]])", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = "<root><ADL><RecordSet><Field>10</Field></RecordSet><RecordSet><Field>20</Field></RecordSet><scalar>2</scalar><result></result></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "32";

            GetScalarValueFromEnvironment(result.Environment, "result", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateActivity_ValidFunction_Expected_EvalPerformed_ValidDateTimeFunction()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = @"now()", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = "<root><ADL><RecordSet><Field>10</Field></RecordSet><RecordSet><Field>20</Field></RecordSet><scalar>2</scalar><result></result></ADL></root>";
            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "result", out string entry, out string error);

            // remove test datalist ;)
            var a = DateTime.ParseExact(entry, GlobalConstants.Dev2DotNetDefaultDateTimeFormat, CultureInfo.InvariantCulture);
        }

        //Bug 6438
        [TestMethod]
        public void CalculateActivity_ConcatenateScalar_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "Concatenate([[testVar]], \"moreText\")", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testVar></testVar><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testVar>ATest</testVar><NewTestVar></NewTestVar></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "ATestmoreText";

            GetScalarValueFromEnvironment(result.Environment, "NewTestVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void CalculateActivity_RightScalar_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "Right([[testVar]], 2)", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testVar></testVar><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testVar>ATest</testVar><NewTestVar></NewTestVar></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "st";

            GetScalarValueFromEnvironment(result.Environment, "NewTestVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void CalculateActivity_LeftScalar_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "Left([[testVar]], 2)", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testVar></testVar><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testVar>ATest</testVar><NewTestVar></NewTestVar></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "AT";

            GetScalarValueFromEnvironment(result.Environment, "NewTestVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateActivity_ConcatenateRecSet_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "Concatenate([[testRecSet(1).testField]], \"moreText\")", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testRecSet><testField></testField></testRecSet><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testRecSet><testField>ATest</testField></testRecSet><NewTestVar></NewTestVar></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "ATestmoreText";

            GetScalarValueFromEnvironment(result.Environment, "NewTestVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        // Bug 8467 - Travis.Frisinger
        [TestMethod]
        public void CalculateActivity_RecordsetWithStar_Expected_SumOf10()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "sum([[rec(*).val]])", Result = "[[sumResult]]" }
            };

            CurrentDl = "<ADL><rec><val></val></rec><sumResult></sumResult></ADL>";
            TestData = "<root><ADL><rec><val>1</val></rec><rec><val>2</val></rec><rec><val>3</val></rec><rec><val>4</val></rec></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "10";

            GetScalarValueFromEnvironment(result.Environment, "sumResult", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateActivity_MultRecordsetWithStar_Expected_SumOf20()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "sum([[rec(*).val]],[[rec(*).val2]])", Result = "[[sumResult]]" }
            };

            CurrentDl = "<ADL><rec><val></val><val2/></rec><sumResult></sumResult></ADL>";
            TestData = "<root><ADL><rec><val>1</val><val2>10</val2></rec><rec><val>2</val><val2>0</val2></rec><rec><val>3</val><val2>0</val2></rec><rec><val>4</val><val2>0</val2></rec></ADL></root>";
            var result = ExecuteProcess();
            const string expected = "20";

            GetScalarValueFromEnvironment(result.Environment, "sumResult", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateActivity_MultRecordsetWithStar_WhenMissingValuesError_Expected_SumOf20()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetAggregateCalculateActivity { Expression = "sum([[rec(*).val]],[[rec(*).val2]])", Result = "[[sumResult]]" }
            };

            CurrentDl = "<ADL><rec><val></val><val2/></rec><sumResult></sumResult></ADL>";
            TestData = "<root><ADL><rec><val>1</val><val2>10</val2></rec><rec><val>2</val></rec><rec><val>3</val></rec><rec><val>4</val></rec></ADL></root>";
            var result = ExecuteProcess();

            try
            {
                GetScalarValueFromEnvironment(result.Environment, "sumResult", out string actual, out string error);
            }
            catch(Exception e)
            {
                StringAssert.Contains(e.Message, "No Value assigned for: [[sumResult]]");
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_GetForEachInputs")]
        public void DsfCalculateActivity_GetForEachInputs_NullContext_EmptyList()
        {
            //------------Setup for test--------------------------
            var dsfCalculateActivity = new DsfDotNetAggregateCalculateActivity();
            //------------Execute Test---------------------------
            var dsfForEachItems = dsfCalculateActivity.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.IsFalse(dsfForEachItems.Any());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_GetForEachInputs")]
        public void DsfCalculateActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(expression, dsfForEachItems[0].Name);
            Assert.AreEqual(expression, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachInputs")]
        public void DsfCalculateActivity_UpdateForEachInputs_GivenNullUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression, act.Expression);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachInputs")]
        public void DsfCalculateActivity_UpdateForEachInputs_GivenMoreThanOneUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(expression, act.Expression);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachInputs")]
        public void DsfCalculateActivity_UpdateForEachInputs_GivenOneUpdate_UpdatesExpressionToItem2InTuple()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Expression);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachOutputs")]
        public void DsfCalculateActivity_UpdateForEachOutputs_GivenNullUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression, act.Expression);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachOutputs")]
        public void DsfCalculateActivity_UpdateForEachOutputs_GivenMoreThanOneUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(expression, act.Expression);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachOutputs")]
        public void DsfCalculateActivity_UpdateForEachOutputs_GivenOneUpdate_UpdatesExpressionToItem2InTuple()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDotNetAggregateCalculateActivity_Equality")]
        public void DsfDotNetAggregateCalculateActivity_Equal()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act1 = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            var act2 = new DsfDotNetAggregateCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(act1.Equals(act2));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDotNetAggregateCalculateActivity_Equality")]
        public void DsfDotNetAggregateCalculateActivity_NotEqual()
        {
            //------------Setup for test--------------------------
            const string expression1 = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string expression2 = "[[Numeric(1).num]]";
            const string result = "[[res]]";
            var act1 = new DsfDotNetAggregateCalculateActivity { Expression = expression1, Result = result };
            var act2 = new DsfDotNetAggregateCalculateActivity { Expression = expression2, Result = result };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(act1.Equals(act2));
        }
    }
}