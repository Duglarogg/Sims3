using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class MethodStore
    {
        string mAssemblyName;
        string mClassName;
        string mRoutineName;
        Type[] mParameters;

        StringBuilder mError = new StringBuilder();

        MethodInfo mMethod;
        bool mChecked;

        public MethodStore(string assemblyName, string className, string routineName, Type[] parameters)
        {
            mAssemblyName = assemblyName;
            mClassName = className;
            mRoutineName = routineName;
            mParameters = parameters;
        }

        public MethodStore(string methodName, Type[] parameters)
        {
            if ((methodName != null) && (methodName.Contains(",")))
            {
                string[] strArray = methodName.Split(new char[] { ',' });

                mClassName = strArray[0];
                mAssemblyName = strArray[1];
                mRoutineName = strArray[2].Replace(" ", "");
            }

            mParameters = parameters;
        }

        public bool Valid
        {
            get { return LookupRoutine(); }
        }

        public string Error
        {
            get
            {
                LookupRoutine();
                return mError.ToString();
            }
        }

        public MethodInfo Method
        {
            get
            {
                LookupRoutine();
                return mMethod;
            }
        }

        public override string ToString()
        {
            return mMethod + " (" + mAssemblyName + "." + mClassName + "." + mRoutineName + ")";
        }

        protected bool LookupRoutine()
        {
            if (!mChecked)
            {
                mError.AppendLine("Assembly: " + mAssemblyName + "\n" + "ClassName: " + mClassName + "\n" + "RoutineName: " + mRoutineName);

                try
                {
                    mChecked = true;

                    if (!string.IsNullOrEmpty(mAssemblyName))
                    {
                        Assembly assembly = AssemblyCheck.FindAssembly(mAssemblyName);

                        if (assembly != null)
                        {
                            mError.AppendLine(" Assembly Found: " + assembly.FullName);

                            Type type = assembly.GetType(mClassName);

                            if (type != null)
                            {
                                mError.AppendLine(" Class Found: " + type.ToString());

                                if (mParameters != null)
                                {
                                    mMethod = type.GetMethod(mRoutineName, mParameters);
                                }
                                else
                                {
                                    mMethod = type.GetMethod(mRoutineName);
                                }

                                if (mMethod != null)
                                {
                                    mError.AppendLine(" Rounte Found");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mError.AppendLine("Exception");
                    Logger.WriteExceptionLog(e, this, mError.ToString());
                }
                finally
                { }
            }

            return (mMethod != null);
        }

        public T Invoke<T>(object[] parameters)
        {
            return Invoke<T>(null, parameters);
        }

        public T Invoke<T>(object obj, object[] parameters)
        {
            if (!Valid) return default(T);

            try
            {
                return (T)mMethod.Invoke(obj, parameters);
            }
            catch (Exception e)
            {
                StringBuilder msg = new StringBuilder();

                int leftSize = 0;

                if (parameters != null)
                {
                    leftSize = parameters.Length;
                }

                int rightSize = 0;

                if (mParameters != null)
                {
                    rightSize = mParameters.Length;
                }

                if (leftSize != rightSize)
                {
                    msg.AppendLine(" Not Enough Parameters: " + leftSize + " != " + rightSize);
                }
                else
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        msg.AppendLine(" Param " + i + ": " + parameters[i].GetType() + " : " + mParameters[i]);
                    }
                }

                Logger.WriteExceptionLog(e, this, msg.ToString());
                return default(T);
            }
        }
    }
}
