using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinalTask.Task1.ThirdParty;

namespace FinalTask.Task1
{
    public class TablePrinter : ICommand
    {
        private readonly IView view;
        private readonly IDatabaseManager manager;

        private const string SupportedTableName = "print ";
        private const int CommandParametersCount = 2;
        private const int TableNameLocationIndexInParameters = 1;

        public TablePrinter(IView view, IDatabaseManager manager)
        {
            this.view = view;
            this.manager = manager;
        }

        public bool CanProcess(string command)
        {
            return command.StartsWith(SupportedTableName);
        }

        public void Process(string command)
        {
            var tableName = GetTableName(command);

            var dataSets = GetTableDataSets(tableName);

            StartProcess(tableName, dataSets);
        }

        private string GetTableName(string command)
        {
            string[] commandParameters = SplitCommand(command);

            ValidateCommandParameters(commandParameters);

            return commandParameters[TableNameLocationIndexInParameters];
        }

        private void ValidateCommandParameters(string[] commandParameters)
        {
            if (IsIncorrectParameters(commandParameters))
            {
                throw new ArgumentException(
                    "incorrect number of parameters. Expected 1, but is " + (commandParameters.Length - 1));
            }
        }

        private IList<IDataSet> GetTableDataSets(string tableName)
        {
            return manager.GetTableData(tableName);
        }

        private bool IsIncorrectParameters(string[] commandPart)
        {
            return commandPart.Length != CommandParametersCount;
        }

        private string[] SplitCommand(string command)
        {
            return command.Split(' ');
        }

        private void StartProcess(string tableName, IList<IDataSet> dataSets)
        {
            var printableString = GetTableAsString(tableName, dataSets);

            WriteToView(printableString);
        }

        private void WriteToView(string value)
        {
            view.Write(value);
        }

        private string GetTableAsString(string tableName, IList<IDataSet> dataSets)
        {
            return IsTableDataSetsEmpty(dataSets) ? GetEmptyTableAsString(tableName) : GetFullTableAsString(dataSets);
        }

        private bool IsTableDataSetsEmpty(IList<IDataSet> dataSets)
        {
            var maxColumnSize = GetMaxColumnSize(dataSets);

            return maxColumnSize == 0;
        }

        private string GetFullTableAsString(IList<IDataSet> dataSets)
        {
            var (columnCount, maxColumnSize) = GetColmnCountAndColumnSize(dataSets);

            var header = GetHeaderOfTheTable(dataSets, columnCount, maxColumnSize);

            var table = GetTableDataAsString(dataSets, columnCount, maxColumnSize);

            return string.Concat(header, table);
        }

        private int GetMaxColumnSize(IList<IDataSet> dataSets)
        {
            return IsNotEmpty(dataSets) ? Math.Max(GetMaxColumnNameLength(dataSets), GetMaxDataSetsValueLength(dataSets)) : 0;
        }

        private int GetMaxColumnNameLength(IList<IDataSet> dataSets)
        {
            return GetDataSetColumnNames(dataSets).Max(m => m.Length);
        }

        private IList<string> GetDataSetColumnNames(IList<IDataSet> dataSets)
        {
            return dataSets[0].GetColumnNames();
        }

        private int GetMaxDataSetsValueLength(IList<IDataSet> dataSets)
        {
            return dataSets.SelectMany(s => s.GetValues()).Max(m => m.ToString().Length);
        }

        private bool IsNotEmpty(IList<IDataSet> dataSets) => dataSets.Count > 0;

        private int GetColumnCount(IList<IDataSet> dataSets)
        {
            return IsNotEmpty(dataSets) ? GetDataSetColumnNames(dataSets).Count : 0;
        }

        private string GetEmptyTableAsString(string tableName)
        {
            StringBuilder result = new StringBuilder();

            var (textEmptyTable, iterations) = GetEmptyTableTextAndIterations(tableName);

            var horizontalBorder = IterateElement(iterations, Boundary.HorizontalBorder);

            result.Append(Boundary.Top.LeftAngle);

            result.Append(horizontalBorder);

            result.Append(Boundary.Top.RightAngle);

            result.Append(Boundary.NewLine);

            result.Append(textEmptyTable);
            
            result.Append(Boundary.NewLine);

            result.Append(Boundary.Bottom.LeftAngle);

            result.Append(horizontalBorder);

            result.Append(Boundary.Bottom.RightAngle);

            result.Append(Boundary.NewLine);

            return result.ToString();
        }

        private string GetTableDataAsString(IList<IDataSet> dataSets, int columnCount, int maxColumnSize)
        {
            StringBuilder result = new StringBuilder();

            for (var currentRow = 0; currentRow < dataSets.Count; currentRow++)
            {
                var values = dataSets[currentRow].GetValues();

                result.Append(Boundary.VerticalBorder);

                for (var column = 0; column < columnCount; column++)
                {
                    (int valuesLength, int iterations) = GetValueLengthAndIterations(maxColumnSize, values[column].ToString());

                    result.Append(IterateElementAndAppend(iterations, Boundary.EmptySpace, values[column].ToString()));

                    IncrementIterations(valuesLength, ref iterations);

                    result.Append(IterateElementAndAppend(iterations, Boundary.EmptySpace, Boundary.VerticalBorder));
                }

                result.Append(Boundary.NewLine);

                if (HasTableDataSetsMoreRows(currentRow, dataSets))
                {
                    result.Append(GetMiddleLineElements(columnCount, maxColumnSize));
                }
            }

            result.Append(GetBottomLineElements(columnCount, maxColumnSize));

            return result.ToString();
        }

        private string GetHeaderOfTheTable(IList<IDataSet> dataSets, int columnCount, int maxColumnSize)
        {
            StringBuilder result = new StringBuilder();

            result.Append(GetTopLineElements(columnCount, maxColumnSize));

            var columnNames = GetDataSetColumnNames(dataSets);

            for (var column = 0; column < columnCount; column++)
            {
                (int columnNamesLength, int iterations) = GetValueLengthAndIterations(maxColumnSize, columnNames[column]);

                result.Append(Boundary.VerticalBorder);

                result.Append(IterateElementAndAppend(iterations, Boundary.EmptySpace, columnNames[column]));

                IncrementIterations(columnNamesLength, ref iterations);

                result.Append(IterateElement(iterations, Boundary.EmptySpace));
            }

            result.Append(Boundary.VerticalBorder);
            result.Append(Boundary.NewLine);

            if (IsNotEmpty(dataSets))
            {
                result.Append(GetMiddleLineElements(columnCount, maxColumnSize));
            }
            else
            {
                result.Append(GetBottomLineElements(columnCount, maxColumnSize));
            }

            return result.ToString();
        }

        private (string, int) GetEmptyTableTextAndIterations(string tableName)
        {
            var textEmptyTable = $"║ Table '{tableName}' is empty or does not exist ║";

            int iterations = (textEmptyTable.Length - 2);

            return (textEmptyTable, iterations);
        }

        private (int, int) GetValueLengthAndIterations(int maxColumnSize, string value)
        {
            var valueLength = value.Length;

            var iterations = (maxColumnSize - valueLength) / 2;

            return (valueLength, iterations);
        }

        private (int, int) GetColmnCountAndColumnSize(IList<IDataSet> dataSets)
        {
            var columnCount = GetColumnCount(dataSets);

            var maxColumnSize = GetColumnSize(dataSets);

            return (columnCount, maxColumnSize);
        }

        private bool IsNotEvenNumber(int columnNamesLength)
        {
            return columnNamesLength % 2 != 0;
        }

        private int GetColumnSize(IList<IDataSet> dataSets)
        {
            var maxColumnSize = GetMaxColumnSize(dataSets);

            return IsNotEvenNumber(maxColumnSize) ? maxColumnSize += 3 : maxColumnSize += 2;
        }

        private void IncrementIterations(int valuesLength, ref int iterations)
        {
            if (IsNotEvenNumber(valuesLength))
            {
                iterations++;
            }
        }

        private bool HasTableDataSetsMoreRows(int currentRow, IList<IDataSet> dataSets) => (dataSets.Count - 1) > currentRow;

        private static int GetIterationsByColumnCount(int columnCount)
        {
            return --columnCount;
        }

        private string IterateElement(int iterations, string elementToIterate)
        {
            StringBuilder result = new StringBuilder();

            for (var i = 0; i < iterations; i++)
            {
                result.Append(elementToIterate);
            }

            return result.ToString();
        }

        private string IterateElementAndAppend(int iterations, string elementToIterate, string appendElement)
        {
            StringBuilder result = new StringBuilder();

            result.Append(IterateElement(iterations, elementToIterate));

            result.Append(appendElement);

            return result.ToString();
        }

        private string GetTopLineElements(int columnCount, int maxColumnSize)
        {
            return GetLineElements(columnCount, maxColumnSize, Boundary.Top);
        }

        private string GetMiddleLineElements(int columnCount, int maxColumnSize)
        {
            return GetLineElements(columnCount, maxColumnSize, Boundary.Middle);
        }

        private string GetBottomLineElements(int columnCount, int maxColumnSize)
        {
            return GetLineElements(columnCount, maxColumnSize, Boundary.Bottom);
        }

        private string GetLineElements(int columnCount, int maxColumnSize, Boundary boundary)
        {
            StringBuilder result = new StringBuilder();

            int iterations = GetIterationsByColumnCount(columnCount);

            string elementToIterate = IterateElementAndAppend(maxColumnSize, Boundary.HorizontalBorder, boundary.MiddleAngle);

            result.Append(boundary.LeftAngle);
          
            result.Append(IterateElement(iterations, elementToIterate));

            result.Append(IterateElementAndAppend(maxColumnSize, Boundary.HorizontalBorder, boundary.RightAngle));

            result.Append(Boundary.NewLine);

            return result.ToString();
        }
    }
}
