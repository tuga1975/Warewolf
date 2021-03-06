/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;




namespace Warewolf.Storage
{
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        DataStorage.WarewolfEnvironment _env;

        public ExecutionEnvironment()
        {
            _env = PublicFunctions.CreateEnv(@"");
            Errors = new HashSet<string>();
            AllErrors = new HashSet<string>();
        }

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update) => Eval(exp, update, false, false);

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists) => Eval(exp, update, throwsifnotexists, false);

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists, bool shouldEscape)
        {
            try
            {
                return PublicFunctions.EvalEnvExpression(exp, update, shouldEscape, _env);
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (throwsifnotexists || e is IndexOutOfRangeException || e.Message.Contains(@"index was not an int"))
                {
                    throw;
                }

                return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            }
        }

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp) => EvalForJson(exp, false);

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp, bool shouldEscape)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            }
            try
            {
                return PublicFunctions.EvalEnvExpression(exp, 0, shouldEscape, _env);
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }
            catch (Exception)
            {
                if (!IsRecordsetIdentifier(exp))
                {
                    return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
                }

                var res = new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.Nothing);
                res.AddNothing();
                return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(res);
            }
        }

        public void AddToJsonObjects(string exp, JContainer jContainer)
        {
            _env = WarewolfDataEvaluationCommon.addToJsonObjects(_env, exp, jContainer);
        }

        public IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update) => DataMergeFunctions.evalForDataMerge(_env, update, exp);

        public void AssignUnique(IEnumerable<string> distinctList, IEnumerable<string> valueList, IEnumerable<string> resList, int update)
        {
            var output = Distinct.evalDistinct(_env, distinctList, valueList, update, resList);
            _env = output;
        }

        public CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update)
        {
            var res = Eval(exp, update);
            if (IsNothing(res))
            {
                throw new NullValueInVariableException($"The expression {exp} has no value assigned.", exp);
            }

            return res;
        }

        public void Assign(string exp, string value, int update)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return;
            }
            try
            {
                var envTemp = PublicFunctions.EvalAssignWithFrame(new AssignValue(exp, value), update, _env);

                _env = envTemp;
                CommitAssign();
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
            }
        }

        public void AssignStrict(string exp, string value, int update)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return;
            }

            try
            {
                var envTemp = PublicFunctions.EvalAssignWithFrameStrict(new AssignValue(exp, value), update, _env);

                _env = envTemp;
                CommitAssign();
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
            }
        }


        public void AssignWithFrame(IAssignValue values, int update)
        {
            try
            {

                var envTemp = PublicFunctions.EvalAssignWithFrame(values, update, _env);
                _env = envTemp;

            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
                throw;
            }
        }
        public void AssignWithFrame(IEnumerable<IAssignValue> values, int update)
        {
            foreach (var value in values)
            {
                AssignWithFrame(value, update);
            }
        }
        public int GetLength(string recordSetName) => _env.RecordSets[recordSetName.Trim()].LastIndex;

        public int GetObjectLength(string recordSetName)
        {
            var trimStart = recordSetName.TrimStart('@');
            var count = _env.JsonObjects[trimStart].Count;
            return count;
        }

        public int GetCount(string recordSetName) => _env.RecordSets[recordSetName.Trim()].Count;

        public IList<int> EvalRecordSetIndexes(string recordsetName, int update) => PublicFunctions.GetIndexes(recordsetName, update, _env).ToList();

        public bool HasRecordSet(string recordsetName)
        {
            var x = EvaluationFunctions.parseLanguageExpression(recordsetName, 0);
            if (x.IsRecordSetNameExpression)
            {
                var recsetName = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
                return _env.RecordSets.ContainsKey(recsetName?.Item.Name);
            }
            return false;
        }

        public IList<string> EvalAsListOfStrings(string expression, int update)
        {
            var result = Eval(expression, update);
            if (result.IsWarewolfAtomResult)
            {
                var x = (result as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult)?.Item;
                return new List<string> { WarewolfAtomToString(x) };
            }
            if (result.IsWarewolfRecordSetResult)
            {
                var recSetResult = result as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                var recSetData = recSetResult?.Item;
                if (recSetData != null)
                {
                    return EvalListOfStringsHelper(recSetData);
                }
            }
            var warewolfAtomListresult = result as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            if (warewolfAtomListresult == null)
            {
                throw new Exception(string.Format(ErrorResource.CouldNotRetrieveStringsFromExpression, expression));
            }

            var item = warewolfAtomListresult.Item;
            return item.Select(WarewolfAtomToString).ToList();
        }

        private static IList<string> EvalListOfStringsHelper(DataStorage.WarewolfRecordset recSetData)
        {
            var data = recSetData.Data.ToArray();
            var listOfData = new List<string>();
            foreach (var keyValuePair in data)
            {
                if (keyValuePair.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                listOfData.AddRange(keyValuePair.Value.Select(WarewolfAtomToString).ToList());
            }
            return listOfData;
        }

        public static string WarewolfAtomToString(DataStorage.WarewolfAtom a) => a?.ToString() ?? string.Empty;

        public static string WarewolfAtomToStringNullAsNothing(DataStorage.WarewolfAtom a) => a == null ? null : (a.IsNothing ? null : a.ToString());

        public static string WarewolfAtomToStringErrorIfNull(DataStorage.WarewolfAtom a)
        {
            if (a == null)
            {
                return string.Empty;
            }

            if (a.IsNothing)
            {
                throw new NullValueInVariableException(ErrorResource.VariableIsNull, string.Empty);
            }
            return a.ToString();
        }

        public static bool IsRecordSetName(string a)
        {
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(a, 0);
                return x.IsRecordSetNameExpression;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidVariableExpression(string expression, out string errorMessage, int update)
        {
            errorMessage = string.Empty;
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(expression, update);
                if (x.IsRecordSetExpression || x.IsScalarExpression || x.IsJsonIdentifierExpression ||
                    x.IsRecordSetNameExpression)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            return false;
        }

        public static string WarewolfEvalResultToString(CommonFunctions.WarewolfEvalResult result)
        {
            if (result.IsWarewolfAtomResult)
            {
                if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
                {
                    var x = warewolfAtomResult.Item;
                    if (x.IsNothing)
                    {
                        return null;
                    }

                    return WarewolfAtomToStringErrorIfNull(x);
                }
                throw new Exception(@"Null when value should have been returned.");
            }
            if (result.IsWarewolfRecordSetResult)
            {
                var recSetResult = result as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                var recSetData = recSetResult?.Item;
                if (recSetData != null)
                {
                    return WarewolfEvalResultToStringHelper(recSetData);
                }
            }
            if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult)
            {
                var x = warewolfAtomListresult.Item;
                var res = new StringBuilder();
                for (int index = 0; index < x.Count; index++)
                {
                    var warewolfAtom = x[index];
                    if (index == x.Count - 1)
                    {
                        res.Append(warewolfAtom);
                    }
                    else
                    {
                        res.Append(warewolfAtom).Append(@",");
                    }
                }
                return res.ToString();
            }
            throw new Exception(@"Null when value should have been returned.");
        }

        private static string WarewolfEvalResultToStringHelper(DataStorage.WarewolfRecordset recSetData)
        {
            var data = recSetData.Data.ToArray();
            var listOfData = new List<string>();
            foreach (var keyValuePair in data)
            {
                if (keyValuePair.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                listOfData.AddRange(keyValuePair.Value.Select(WarewolfAtomToString).ToList());
            }
            return string.Join(",", listOfData);
        }

        public void EvalAssignFromNestedStar(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            AssignWithFrameAndList(exp, recsetResult.Item, false, update);
        }

        public void EvalAssignFromNestedLast(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            var exists = PublicFunctions.RecordsetExpressionExists(exp, _env);
            if (!exists)
            {
                exp = ToStar(exp);
            }

            AssignWithFrameAndList(exp, recsetResult.Item, exists, update);
        }

        public void AssignWithFrameAndList(string assignValue, IEnumerable<DataStorage.WarewolfAtom> item, bool shouldUseLast, int update)
        {
            _env = PublicFunctions.EvalAssignFromList(assignValue, item, _env, update, shouldUseLast);
        }

        public void EvalAssignFromNestedNumeric(string rawValue, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            if (recsetResult.Item.Any())
            {
                AssignWithFrame(new AssignValue(rawValue, WarewolfAtomToString(recsetResult.Item.Last())), update);
            }
        }

        public void EvalDelete(string exp, int update)
    {
            _env = PublicFunctions.EvalDelete(exp, update, _env);
        }

        public void CommitAssign()
        {
            _env = PublicFunctions.RemoveFraming(_env);
        }

        public void SortRecordSet(string sortField, bool descOrder, int update)
        {
            _env = PublicFunctions.SortRecset(sortField, descOrder, update, _env);
        }

        public string ToStar(string expression)
        {
            var exp = EvaluationFunctions.parseLanguageExpression(expression, 0);
            if (exp.IsRecordSetExpression && exp is LanguageAST.LanguageExpression.RecordSetExpression rec)
            {
                return $"[[{rec.Item.Name}(*).{rec.Item.Column}]]";
            }

            if (exp.IsRecordSetNameExpression && exp is LanguageAST.LanguageExpression.RecordSetNameExpression recNameExp)
            {
                return $"[[{recNameExp.Item.Name}(*)]]";
            }

            if (exp.IsJsonIdentifierExpression)
            {
                var replace = expression.Replace(@"()", @"(*)");
                return replace;
            }
            return expression;
        }
        public IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update) => EvalAsTable(recordsetExpression, update, false);
        public IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update, bool throwsifnotexists)

        {           
            var result = PublicFunctions.EvalEnvExpressionToTable(recordsetExpression, update, _env, throwsifnotexists);
            return result;
        }

        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update) => EvalAsList(expression, update, false);
        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update, bool throwsifnotexists)
        {
            var result = Eval(expression, update, throwsifnotexists);
            if (result.IsWarewolfAtomResult)
            {
                var warewolfAtomResult = result as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (warewolfAtomResult == null)
                {
                    throw new Exception(@"Null when value should have been returned.");
                }

                var item = warewolfAtomResult.Item;
                return new List<DataStorage.WarewolfAtom> { item };
            }
            if (result.IsWarewolfRecordSetResult)
            {
                var recSetResult = result as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                var recSetData = recSetResult?.Item;
                if (recSetData != null)
                {
                    return EvalAsListHelper(recSetData);
                }
            }
            var x = (result as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)?.Item;
            return x?.ToList();
        }

        private static IEnumerable<DataStorage.WarewolfAtom> EvalAsListHelper(DataStorage.WarewolfRecordset recSetData)
        {
            var data = recSetData.Data.ToArray();
            var listOfData = new List<DataStorage.WarewolfAtom>();
            foreach (var keyValuePair in data)
            {
                if (keyValuePair.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                listOfData.AddRange(keyValuePair.Value.ToList());
            }
            return listOfData;
        }

        public IEnumerable<int> EvalWhere(string expression, Func<DataStorage.WarewolfAtom, bool> clause, int update) => PublicFunctions.EvalWhere(expression, _env, update, clause);

        public void ApplyUpdate(string expression, Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> clause, int update)
        {

            var temp = PublicFunctions.EvalUpdate(expression, _env, update, clause);
            _env = temp;
        }

        public HashSet<string> Errors { get; }

        public HashSet<string> AllErrors { get; }

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AssignDataShape(string p)
        {
            var env = PublicFunctions.EvalDataShape(p, _env);
            _env = env;
        }

        public string FetchErrors() => string.Join(Environment.NewLine, AllErrors.Union(Errors));

        public bool HasErrors() => Errors.Count(s => !string.IsNullOrEmpty(s)) + AllErrors.Count(s => !string.IsNullOrEmpty(s)) > 0;

        public string EvalToExpression(string exp, int update) => string.IsNullOrEmpty(exp) ? string.Empty : EvaluationFunctions.evalToExpression(_env, update, exp);

        public static string ConvertToIndex(string outputVar, int i)
        {
            var output = EvaluationFunctions.parseLanguageExpression(outputVar, 0);
            if (output.IsRecordSetExpression)
            {
                var outputidentifier = (output as LanguageAST.LanguageExpression.RecordSetExpression)?.Item;
                if (Equals(outputidentifier?.Index, LanguageAST.Index.Star))
                {
                    return $"[[{outputidentifier?.Name}({i}).{outputidentifier?.Column}]]";
                }
            }
            return outputVar;
        }

        public static bool IsRecordsetIdentifier(string assignVar)
        {
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(assignVar, 0);
                return x.IsRecordSetExpression;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsScalar(string assignVar)
        {
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(assignVar, 0);
                return x.IsScalarExpression;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsNothing(CommonFunctions.WarewolfEvalResult evalInp1) => CommonFunctions.isNothing(evalInp1);

        public static string GetPositionColumnExpression(string recordset)
        {
            var rec = EvaluationFunctions.parseLanguageExpression(recordset, 0);
            if (!rec.IsRecordSetExpression && !rec.IsRecordSetNameExpression)
            {
                return recordset;
            }

            var recordSetExpression = rec as LanguageAST.LanguageExpression.RecordSetExpression;
            var recordSetNameExpression = rec as LanguageAST.LanguageExpression.RecordSetNameExpression;

            var index = recordSetExpression?.Item?.Name ?? recordSetNameExpression?.Item?.Name;
            return $"[[{index}(*).{EvaluationFunctions.PositionColumn}]]";
        }

        public static bool IsValidRecordSetIndex(string exp) => PublicFunctions.IsValidRecsetExpression(exp);

        public void AssignJson(IEnumerable<IAssignValue> values, int update)
        {
            foreach (var value in values)
            {
                AssignJson(value, update);
            }
        }

        public void AssignJson(IAssignValue value, int update)
        {
            try
            {
                if (string.IsNullOrEmpty(value.Name))
                {
                    return;
                }
                var envTemp = AssignEvaluation.evalJsonAssign(value, update, _env);
                _env = envTemp;
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
                throw;
            }
        }

        public JContainer EvalJContainer(string exp)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return null;
            }

            var var = EvaluationFunctions.parseLanguageExpressionWithoutUpdate(exp);
            if (!var.IsJsonIdentifierExpression)
            {
                return null;
            }

            var jsonIdentifierExpression = var as LanguageAST.LanguageExpression.JsonIdentifierExpression;
            if (jsonIdentifierExpression?.Item is LanguageAST.JsonIdentifierExpression.NameExpression nameExpression)
            {
                return _env.JsonObjects[nameExpression.Item.Name];
            }
            if (jsonIdentifierExpression?.Item is LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression arrayExpression)
            {
                return _env.JsonObjects[arrayExpression.Item.ObjectName];
            }
            return null;
        }

        public List<string> GetIndexes(string exp)
        {
            var indexMap = new List<string>();
            if (!string.IsNullOrEmpty(exp))
            {
                var var = EvaluationFunctions.parseLanguageExpressionWithoutUpdate(exp);

                if (var.IsJsonIdentifierExpression)
                {
                    if (var is LanguageAST.LanguageExpression.JsonIdentifierExpression jsonIdentifierExpression)
                    {
                        BuildIndexMap(jsonIdentifierExpression.Item, exp, indexMap, null);
                    }
                }
                else
                {
                    if (var.IsRecordSetExpression && var is LanguageAST.LanguageExpression.RecordSetExpression recSetExpression)
                    {
                        GetIndexAddRecordSetIndexes(exp, indexMap, recSetExpression);
                    }

                }
            }
            return indexMap.Where(s => !s.Contains(@"(*)")).ToList();
        }

        private void GetIndexAddRecordSetIndexes(string exp, List<string> indexMap, LanguageAST.LanguageExpression.RecordSetExpression recSetExpression)
        {
            var indexes = EvalRecordSetIndexes(@"[[" + recSetExpression.Item.Name + @"(*)]]", 0);
            foreach (var index in indexes)
            {
                indexMap.Add(exp.Replace(@"(*).", $"({index})."));
            }
        }

        void BuildIndexMap(LanguageAST.JsonIdentifierExpression var, string exp, List<string> indexMap, JContainer container)
        {
            var jsonIdentifierExpression = var;
            if (jsonIdentifierExpression == null)
            {
                return;
            }
            if (jsonIdentifierExpression is LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression)
            {
                var objectName = nameExpression.Item.ObjectName;
                JContainer obj;
                JArray arr = null;
                if (container == null)
                {
                    obj = _env.JsonObjects[objectName];
                    arr = obj as JArray;
                }
                else
                {
                    var props = container.FirstOrDefault(token => token.Type == JTokenType.Property && ((JProperty)token).Name == objectName);
                    if (props != null)
                    {
                        obj = props.First as JContainer;
                        arr = obj as JArray;
                    }
                    else
                    {
                        obj = container;
                    }
                }

                if (arr != null)
                {
                    BuildIndexMapHelper(exp, indexMap, nameExpression, objectName, arr);
                }
                else
                {
                    if (!nameExpression.Item.Next.IsTerminal)
                    {
                        BuildIndexMap(nameExpression.Item.Next, exp, indexMap, obj);
                    }
                }
            }
            else
            {
                if (jsonIdentifierExpression is LanguageAST.JsonIdentifierExpression.NestedNameExpression nestedNameExpression)
                {
                    JContainer obj;
                    var objectName = nestedNameExpression.Item.ObjectName;
                    if (container == null)
                    {
                        obj = _env.JsonObjects[objectName];
                    }
                    else
                    {
                        var props = container.FirstOrDefault(token => token.Type == JTokenType.Property && ((JProperty)token).Name == objectName);
                        obj = props != null ? props.First as JContainer : container;
                    }
                    BuildIndexMap(nestedNameExpression.Item.Next, exp, indexMap, obj);
                }
            }
        }

        private void BuildIndexMapHelper(string exp, List<string> indexMap, LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression, string objectName, JArray arr)
        {
            var indexToInt = AssignEvaluation.indexToInt(LanguageAST.Index.Star, arr).ToList();
            foreach (var i in indexToInt)
            {
                if (!string.IsNullOrEmpty(exp))
                {
                    var indexed = objectName + @"(" + i + @")";
                    var updatedExp = exp.Replace(objectName + @"(*)", indexed);
                    indexMap.Add(updatedExp);
                    BuildIndexMap(nameExpression.Item.Next, updatedExp, indexMap, arr[i - 1] as JContainer);
                }
            }
        }
    }
}