﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectDumper.cs" company="EvePanix">
//   Copyright (c) Jedzia 2001-2012, EvePanix. All rights reserved.
//   See the license notes shipped with this source and the GNU GPL.
// </copyright>
// <author>Jedzia</author>
// <email>jed69@gmx.de</email>
// <date>$date$</date>
// --------------------------------------------------------------------------------------------------------------------

namespace NStub.Core.Util.Dumper
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Object low level dumper.
    /// </summary>
    public class ObjectDumper
    {
        #region Fields

        private readonly int depth;
        private int level;
        private int pos;
        private TextWriter writer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDumper"/> class.
        /// </summary>
        /// <param name="depth">The iteration level.</param>
        /// <param name="writer">The output text writer.</param>
        /// <param name="maxcount">The maximum count of dumps.</param>
        internal ObjectDumper(int depth, int maxcount, TextWriter writer)
        {
            if (depth < 0)
            {
                depth = 0;
            }
            if (depth > 20)
            {
                depth = 20;
            }

            if (maxcount < 0)
            {
                maxcount = 0;
            }

            this.depth = depth;
            this.writer = writer;
            this.maxCount = maxcount;
        }


        /*/// <summary>
        /// Prevents a default instance of the <see cref="ObjectDumper"/> class from being created.
        /// </summary>
        /// <param name="depth">The iteration level.</param>
        private ObjectDumper(int depth)
        {
            this.depth = depth;
        }*/

        /// <summary>
        /// Prevents a default instance of the <see cref="ObjectDumper"/> class from being created.
        /// </summary>
        /// <param name="depth">The iteration level.</param>
        /// <param name="maxcount">The maximum count of dumps.</param>
        private ObjectDumper(int depth, int maxcount)
        {
            if (depth < 0)
            {
                depth = 0;
            }
            if (depth > 20)
            {
                depth = 20;
            }

            if (maxcount < 0)
            {
                maxcount = 0;
            }
            this.depth = depth;
            this.maxCount = maxcount;
        }

        #endregion

        #region Properties

        private TextWriter Writer
        {
            get
            {
                return this.writer;
            }

            /*set
            {
                this.writer = value;
            }*/
        }

        #endregion

        /// <summary>
        /// Writes the specified element to the output.
        /// </summary>
        /// <param name="element">The element.</param>
        public static void Write(object element)
        {
            Write(element, 0);
        }

        /// <summary>
        /// Writes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="depth">The iteration level.</param>
        public static void Write(object element, int depth)
        {
            Write(element, depth, int.MaxValue, Console.Out);
        }

        /// <summary>
        /// Writes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="depth">The iteration level.</param>
        /// <param name="maxcount">The maximum count of dumps.</param>
        public static void Write(object element, int depth, int maxcount)
        {
            Write(element, depth, maxcount, Console.Out);
        }

                /// <summary>
        /// Writes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="depth">The iteration level.</param>
        /// <param name="maxcount">The maximum count of dumps.</param>
        /// <param name="log">The output logger.</param>
        public static void Write(object element, int depth, int maxcount, TextWriter log)
        {
            Write(null, element, depth, maxcount, log);
        }

        /// <summary>
        /// Writes the specified element.
        /// </summary>
        /// <param name="prefix">The prefix to print.</param>
        /// <param name="element">The element to dump.</param>
        /// <param name="depth">The iteration level.</param>
        /// <param name="maxcount">The maximum count of dumps.</param>
        /// <param name="log">The output logger.</param>
        public static void Write(string prefix, object element, int depth, int maxcount, TextWriter log)
        {
            ObjectDumper dumper = new ObjectDumper(depth, maxcount);
            dumper.writer = log;
            // string prefix = null;
            if (element != null)
            {
                dumper.Write("[" + element.GetType().Name + "]" + log.NewLine);
                //prefix = "[" + element.GetType().Name + "]";
            }
            dumper.WriteObject(prefix, element);
        }

        internal void WriteObject(object element)
        {
            string prefix = null;
            if (element != null)
            {
                this.Write("[" + element.GetType().Name + "]" + this.Writer.NewLine);
            }
            this.WriteObject(prefix, element);
        }

        private int maxCounter = 0;
        private readonly int maxCount = int.MaxValue;
        private bool maxCounterReachedPrinted;
        internal void WriteObject(string prefix, object element)
        {
            // WriteIndent();
            // Write(prefix);
            // WriteLine();
            /*if (this.level > this.depth)
            {
                return;
            }*/

            if (this.maxCounter > this.maxCount)
            {
                if (!maxCounterReachedPrinted)
                {
                    maxCounterReachedPrinted = true;
                    this.WriteLine();
                    this.Write("maximum Dump Count of [" + maxCount + "] reached...");
                }
                return;
            }
            maxCounter++;
            /*if (element.GetType().FullName == "System.RuntimeType")
            {
                // this.Write("-System.RuntimeType-");
                // this.level++;
                return;
            }*/

            if (element == null || element is ValueType || element is string)
            {
                this.WriteIndent();
                this.Write(prefix);

                // Write(" [" + element.GetType().FullName + "] ");
                this.WriteValue(element);
                this.WriteLine();
            }
            else
            {
                IEnumerable enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            this.WriteIndent();
                            this.Write(prefix);
                            this.Write("...");
                            this.WriteLine();
                            if (this.level < this.depth)
                            {
                                this.level++;
                                this.WriteObject(prefix, item);
                                this.level--;
                            }
                        }
                        else
                        {
                            this.WriteObject(prefix, item);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    this.WriteIndent();
                    if (prefix != null && members.Length > 0)
                    {
                        this.Write("[" + element.GetType().Name + "]");
                    }

                    this.Write(prefix);
                    bool propWritten = false;
                    foreach (MemberInfo m in members)
                    {
                        FieldInfo f = m as FieldInfo;
                        PropertyInfo p = m as PropertyInfo;
                        if (f != null || p != null)
                        {
                            if (propWritten)
                            {
                                this.WriteTab();
                            }
                            else
                            {
                                propWritten = true;
                            }

                            // Write("[" + element.GetType().Name + "]");
                            this.Write(m.Name);
                            this.Write("=");
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if (t.IsValueType || t == typeof(string))
                            {
                                //try
                                //{
                                if (f != null)
                                {
                                    //if (f.FieldType == typeof(GenericParameterAttributes))
                                    {
                                        //    this.Write(f.Name + " is Generic. ");
                                    }
                                    //else
                                    {
                                        this.WriteValue(f.GetValue(element));
                                    }
                                }
                                else
                                {
                                    //if ((p.GetType() == typeof(GenericParameterAttributes)))
                                    //if (p.PropertyType == typeof(GenericParameterAttributes))
                                    if (p.ToString().Contains("Generic"))
                                    {
                                        // Todo: investigate this ... exception and better type resolution.
                                        //this.Write(p.Name + " is Generic. ");
                                        this.Write("!" + p.ToString() + "!");
                                    }
                                    else
                                    {
                                        this.WriteValue(p.GetValue(element, null));
                                    }
                                }
                                
                                //}
                                //catch (Exception ex)
                                //{
                                //this.WriteLine();
                                //this.Write(ex.Message);
                                //if (ex.InnerException != null)
                                //   this.Write(ex.InnerException.Message);
                                //this.WriteLine();
                                //throw;
                                //return;
                                //}
                            }
                            else
                            {
                                if (typeof(IEnumerable).IsAssignableFrom(t))
                                {
                                    this.Write("...");
                                }
                                else
                                {
                                    this.Write("{ }");
                                }
                            }
                        }
                    }

                    if (propWritten)
                    {
                        this.WriteLine();
                    }

                    if (this.level < this.depth)
                    {
                        foreach (MemberInfo m in members)
                        {
                            FieldInfo f = m as FieldInfo;
                            PropertyInfo p = m as PropertyInfo;
                            if (f != null || p != null)
                            {
                                Type t = f != null ? f.FieldType : p.PropertyType;
                                if (!(t.IsValueType || t == typeof(string)))
                                {
                                    //object value = f != null ? f.GetValue(element) : p.GetValue(element, null);
                                    object value = null;
                                    if (f == null)
                                    {
                                        if (p.ToString().Contains("DeclaringMethod"))
                                        {
                                            // Todo: investigate this ... exception and better type resolution.
                                            this.Write("!" + p.ToString() + "!");
                                        }
                                        else
                                        {
                                           
                                            // var e1 = p.PropertyType.IsGenericParameter;
                                            // var e2 = p.ReflectedType.IsGenericParameter;
                                            // var e3 = p.DeclaringType.IsGenericParameter;
                                            value = p.GetValue(element, null);
                                        }
                                    }
                                    else
                                    {
                                        value = f.GetValue(element);
                                    }

                                    if (value != null)
                                    {
                                        this.level++;
                                        this.WriteObject(m.Name + ": ", value);
                                        this.level--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private void Write(string s)
        {
            if (s != null)
            {
                this.writer.Write(s);
                this.pos += s.Length;
            }
        }

        private void WriteIndent()
        {
            for (int i = 0; i < this.level; i++)
            {
                this.writer.Write("  ");
            }
        }

        private void WriteLine()
        {
            this.writer.WriteLine();
            this.pos = 0;
        }

        private void WriteTab()
        {
            this.Write("  ");
            while (this.pos % 8 != 0)
            {
                this.Write(" ");
            }
        }

        private void WriteValue(object o)
        {
            if (o == null)
            {
                this.Write("null");
            }
            else if (o is DateTime)
            {
                this.Write(((DateTime)o).ToShortDateString());
            }
            else if (o is ValueType || o is string)
            {
                this.Write(o.ToString());
            }
            /*else if (o is IEnumerable)
            {
                this.Write("...");
            }*/
            else
            {
                this.Write("{ }");
                throw new InvalidCastException("WriteValue NOT FOUND");
            }
        }
    }
}