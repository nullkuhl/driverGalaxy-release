using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DUSDK_for.NET
{
    static class MarshalHelper
    {       
 

        /// <summary>
        /// Copies structure array to IntPtr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="managedArray"></param>
        /// <returns></returns>
        unsafe public static IntPtr MarshalArray<T>(ref T[] managedArray)
        {
            if (managedArray == null)
            {
                return IntPtr.Zero;
            }
            int iSizeOfOneDataPos = Marshal.SizeOf(typeof(T));
            int iTotalSize = iSizeOfOneDataPos * managedArray.Length;
            IntPtr pUnmanagedData = Marshal.AllocHGlobal(iTotalSize);
            byte* pbyUnmanagedData = (byte*)(pUnmanagedData.ToPointer());

            for (int i = 0; i < managedArray.Length; i++, pbyUnmanagedData += (iSizeOfOneDataPos))
            {
                IntPtr pOneDataPos = new IntPtr(pbyUnmanagedData);
                Marshal.StructureToPtr(managedArray[i], pOneDataPos, false);
            }

            return pUnmanagedData;
        }


        /// <summary>
        /// Copies IntPtr to structure array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pUnmanagedDatas"></param>
        /// <param name="managedArray"></param>
        unsafe public static void UnMarshalArray<T>(IntPtr pUnmanagedDatas, ref T[] managedArray)
        {
            if (pUnmanagedDatas == IntPtr.Zero)
            {
                return;
            }
            int iSizeOfOneDataPos = Marshal.SizeOf(typeof(T));
            byte* pbyUnmanagedDatas = (byte*)(pUnmanagedDatas.ToPointer());

            for (int i = 0; i < managedArray.Length; i++, pbyUnmanagedDatas += (iSizeOfOneDataPos))
            {
                IntPtr pOneDataPos = new IntPtr(pbyUnmanagedDatas);
                //managedArray[i] = (T)(Marshal.PtrToStructure(pOneDataPos, typeof(T)));                
                managedArray[i] = (T)Marshal.PtrToStructure((IntPtr)(pUnmanagedDatas.ToInt64() + i * Marshal.SizeOf(typeof(T))),
                            typeof(T)
                            );
            }

            return;
        }
    }
}
