using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表达式
/// </summary>
public class ExpressionParser
{
    private const char LeftBracketSymbol = '(';
    private const char RightBracketSymbol = ')';
    private const char AndSymbol = '&';
    private const char OrSymbol = '|';
    private const char NotSymbol = '!';

    private const char FuncSplit = ':';
    private const char ParamSplit = '_';

    public string StackState
    {
        get
        {
            string res = "";
            var arry = _symbolStack.ToArray();
            foreach (var item in arry)
            {
                switch ((Symbol)item)
                {
                    case Symbol.And:
                        res += AndSymbol;
                        break;
                    case Symbol.Or:
                        res += OrSymbol;
                        break;
                    case Symbol.Not:
                        res += NotSymbol;
                        break;
                    case Symbol.LeftBracket:
                        res += LeftBracketSymbol;
                        break;
                    case Symbol.RightBracket:
                        res += RightBracketSymbol;
                        break;

                    default:
                        break;
                }
            }
            return res;
        }
    }

    public enum Symbol
    {
        Function = 0,
        LeftBracket = -1,
        RightBracket = -2,
        Not = -3,
        And = -4,
        Or = -5,

        //下面是用来记录结果的
        True = -2000,

        False = -3000
    }

    public struct FuncParams
    {
        public int Id;

        public int[] Params;

        public FuncParams(int id, int[] p)
        {
            Id = id;
            Params = p;
        }

        public FuncParams(int id)
        {
            Id = id;
            Params = null;
        }
    }

    private readonly Dictionary<string, int> _functions = new Dictionary<string, int>();
    private readonly Stack<int> _symbolStack = new Stack<int>();

    private  Stack<FuncParams> _runningStack = new Stack<FuncParams>();

    public ExpressionParser(Dictionary<string, int> functions)
    {
        _functions = functions;
    }

    public Stack<FuncParams> Parse(string str)
    {
        var stack = new Stack<FuncParams>();

        _symbolStack.Clear();

        int funcEnd = str.Length-1;
        int i = funcEnd;
        for (i =  str.Length -1;i >=0 ; i--)
        {
            switch (str[i])
            {
                case LeftBracketSymbol:
                    TryPushFunction(ref str, ref funcEnd, i, stack);
                    TryPushSymbol(Symbol.LeftBracket, stack, _symbolStack);
                    funcEnd = i - 1;
                    break;

                case RightBracketSymbol:
                    TryPushFunction(ref str, ref funcEnd, i, stack);
                    TryPushSymbol(Symbol.RightBracket, stack, _symbolStack);
                    funcEnd = i - 1;
                    break;

                case AndSymbol:
                    TryPushFunction(ref str, ref funcEnd, i, stack);
                    TryPushSymbol(Symbol.And, stack, _symbolStack);
                    funcEnd = i - 1;
                    break;

                case OrSymbol:
                    TryPushFunction(ref str, ref funcEnd, i, stack);
                    TryPushSymbol(Symbol.Or, stack, _symbolStack);
                    funcEnd = i - 1;
                    break;

                case NotSymbol:
                    TryPushFunction(ref str, ref funcEnd, i, stack);
                    TryPushSymbol(Symbol.Not, stack, _symbolStack);
                    funcEnd = i - 1;
                    break;

                default:
                    break;
            }
        }
        if (funcEnd > i)
        {
            TryPushFunction(ref str, ref funcEnd, i, stack);
        }
        PopAll(stack, _symbolStack);

        return stack;
    }

    private void TryPushFunction(ref string str, ref int funcEnd, int funcStartLast, Stack<FuncParams> stack)
    {
        if (funcEnd > funcStartLast)
        {
            var funcStr = str.Substring(funcStartLast + 1, funcEnd - funcStartLast );
            var strs = funcStr.Split(FuncSplit);
            if (_functions.ContainsKey(strs[0]))
            {
                if (strs.Length <= 1 || string.IsNullOrEmpty(strs[1]))
                {
                    stack.Push(new FuncParams(_functions[strs[0]]));
                }
                else
                {
                    var ps = strs[1].Split(ParamSplit);
                    int[] ints = new int[ps.Length];

                    for (int i = 0; i < ints.Length; i++)
                    {
                        ints[i] = int.Parse(ps[i]);
                    }
                    stack.Push(new FuncParams(_functions[strs[0]], ints));
                }
            }
            else
            {
                Debug.LogError("Dont Find Func - >" + strs[0]);
            }
            funcEnd = funcStartLast ;
        }
    }

    private void TryPushSymbol(Symbol symbol, Stack<FuncParams> stack, Stack<int> symbolStack)
    {
        if (symbol == Symbol.LeftBracket)
        {
            while (symbolStack.Count > 0 && symbolStack.Peek() != (int)Symbol.RightBracket)
            {
                stack.Push(new FuncParams(symbolStack.Pop()));
            }
            if (symbolStack.Count > 0)
            {
                //取出(
                symbolStack.Pop();
            }
        }
        else
        {
            while (symbolStack.Count > 0 && symbolStack.Peek() > (int)symbol && symbolStack.Peek() != (int)Symbol.RightBracket)
            {
                stack.Push(new FuncParams(symbolStack.Pop()));
            }

            symbolStack.Push((int)symbol);
        }
    }

    private void PopAll(Stack<FuncParams> stack, Stack<int> symbolStack)
    {
        while (symbolStack.Count > 0)
        {
            stack.Push(new FuncParams(symbolStack.Pop()));
        }
    }

    public bool Run(Stack<FuncParams> stack, Func<int, int[], bool> func)
    {
        if (stack.Count == 0)
        {
            return true; ;
        }

        _runningStack.Clear();
        var tstack = new Stack<FuncParams>();
        foreach (var f in stack)
        {
            tstack.Push(f);
        }
        //睿智操作，复制一个栈
        foreach (var f in tstack)
        {
            _runningStack.Push(f);
        }

        //Stack<FuncParams> bools = new Stack<FuncParams>();
        return Pop(func);
    }

    //废弃符号
    private void Abandon()
    {
        if (_runningStack.Peek().Id >= 0)
        {
            _runningStack.Pop();
            return ;
        }
        //符号
        switch ((Symbol)_runningStack.Pop().Id)
        {
            case Symbol.And:
                Abandon();
                Abandon();
                break;
            case Symbol.Or:
                Abandon();
                Abandon();
                break;
            case Symbol.Not:
                Abandon();
                break;
            default:
                break;
        }
    }
    private bool Pop(Func<int, int[], bool> boolfunc)
    {
        if (_runningStack.Count == 0)
        {
            return true;// RunFunc(bools.Pop(), boolfunc);
        }

        if (_runningStack.Peek().Id >= 0)
        {
            var func = _runningStack.Pop();
            return RunFunc(func, boolfunc);
        }
        //符号
        switch ((Symbol)_runningStack.Pop().Id)
        {
            case Symbol.And:
 
                if (!Pop(boolfunc))
                {
                    Abandon();
                    return false; 
                }
                else if (!Pop(boolfunc))
                {
                    return false;
                }
                else
                {
                    return true;
                }

            case Symbol.Or:
                if (Pop(boolfunc))
                {
                    Abandon();
                    return true;
                }
                else if (Pop(boolfunc))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case Symbol.Not:
                return !Pop(boolfunc);
           
            default:
                break;
        }
        return Pop(boolfunc);
    }

    private bool RunFunc(FuncParams func, Func<int, int[], bool> boolfunc)
    {
        if (func.Id < 0)
        {
            return func.Id == (int)Symbol.True;
        }
        else
        {
            return boolfunc(func.Id, func.Params);
        }
    }


    public class TableFuncGroup<Tcontext>
    {
        public delegate bool TableFunc(int[] para, Tcontext context);

        private Dictionary<string, int> _nameDic = new Dictionary<string, int>();

        private List<TableFunc> _funcs = new List<TableFunc>();

        private Tcontext _context;

        //存放着根据字符串得到的栈
        Dictionary<string, Dictionary<int, Stack<ExpressionParser.FuncParams>>> _allStack
            = new Dictionary<string, Dictionary<int, Stack<ExpressionParser.FuncParams>>>();
        private object _tableFuncGroup;
        private ExpressionParser _expressionParser;

        //统计方法数量
        public int GetFuncCount(string _funcName, int _type)
        {
            int count = 0;
            new List<FuncParams>(_allStack[_funcName][_type]).ForEach((i)=> {
                if (i.Params != null) {
                    count++;
                }
            });
            return count;
        }

        public Dictionary<string, int> NameDic
        {
            get
            {
                return _nameDic;
            }
        }

        /// <summary>
        /// 构造函数
        ///  收集某个类对象的所有Collect标签的函数
        /// </summary>
        /// <param name="type">The type.</param>
        public TableFuncGroup(System.Object obj)
         {
            //这里是方法
            var methods = obj.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(true);
                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i] is CollectAttribute)
                    {
                        try
                        {
                            var d = System.Delegate.CreateDelegate(typeof(TableFunc), obj, method);
                            //生成到委托
                            Add(method.Name, (TableFunc)d);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(method.Name + ": " + e);
                            throw;
                        }
                      
                      
                        break;
                    }
                }
            }

            _expressionParser = new ExpressionParser(_nameDic);
        }
        public void Parse(string itemName, int id, string expression)
        {
            if (!_allStack.ContainsKey(itemName))
            {
                _allStack[itemName] = new Dictionary<int, Stack<FuncParams>>();
            }
            _allStack[itemName][id] = _expressionParser.Parse(expression);
        }

        /// <summary>
        /// 运行缓存的字符串
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public bool Run(string itemName, int id, Tcontext context)
        {
            Debug.Log("Run :" + itemName + " " + id);
            _context = context;
            return _expressionParser.Run(_allStack[itemName][id], DoFunc); 
        }

        public void ClearContext()
        {
            _context = default(Tcontext);
        }

        private bool DoFunc(int index, int[] para)
        {
            return _funcs[index](para, _context);
        }

        private void Add(string name, TableFunc func)
        {
            NameDic[name] = _funcs.Count;
            _funcs.Add(func);
        }
    }

}

class CollectAttribute:Attribute
{

}