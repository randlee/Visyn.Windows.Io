#region Copyright (c) 2015-2018 Visyn
// The MIT License(MIT)
// 
// Copyright (c) 2015-2018 Visyn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Visyn.Windows.Io.FileHelper.Core;
using Visyn.Windows.Io.FileHelper.Interfaces;


namespace Visyn.Windows.Io.FileHelper
{
    /// <summary>
    /// Base for engine events
    /// </summary>
    /// <typeparam name="T">Specific engine</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class EventEngineBase<T> : EngineBase, IFileHelperEngine<T> where T : class
    {
        /// <summary>
        /// Define an event based on an engine, based on a record
        /// </summary>
        /// <param name="recordType">Type of the record</param>
        protected EventEngineBase(Type recordType) : base(recordType) {}

        /// <summary>
        /// Define an event based on a record with a specific encoding
        /// </summary>
        /// <param name="recordType">Type of the record</param>
        /// <param name="encoding">Encoding specified</param>
        protected EventEngineBase(Type recordType, Encoding encoding) : base(recordType, encoding) {}

        /// <summary>
        /// Event based upon supplied record information
        /// </summary>
        /// <param name="ri"></param>
        internal EventEngineBase(VisynRecordInfo ri) : base(ri) {}

        /// <summary>
        /// Called in read operations just before the record string is
        /// translated to a record.
        /// </summary>
        public event Events.BeforeReadHandler<T> BeforeReadRecord;

        /// <summary>
        /// Called in read operations just after the record was created from a
        /// record string.
        /// </summary>
        public event Events.AfterReadHandler<T> AfterReadRecord;

        /// <summary>
        /// Called in write operations just before the record is converted to a
        /// string to write it.
        /// </summary>
        public event Events.BeforeWriteHandler<T> BeforeWriteRecord;

        /// <summary>
        /// Called in write operations just after the record was converted to a
        /// string.
        /// </summary>
        public event Events.AfterWriteHandler<T> AfterWriteRecord;

        /// <summary>
        /// Check whether we need to notify the read to anyone
        /// </summary>
        protected bool MustNotifyRead => BeforeReadRecord != null ||
                                         AfterReadRecord != null ||
                                         RecordInfo.NotifyRead;

        /// <summary>
        /// Determine whether we have to run notify write on every iteration
        /// </summary>
        protected bool MustNotifyWrite => BeforeWriteRecord != null ||
                                          AfterWriteRecord != null ||
                                          RecordInfo.NotifyWrite;


        /// <summary>
        /// Provide a hook to preprocess a record
        /// </summary>
        /// <param name="e">Record details before read</param>
        /// <returns>True if record to be skipped</returns>
        protected bool OnBeforeReadRecord(Events.BeforeReadEventArgs<T> e)
        {
            if (RecordInfo.NotifyRead) ((INotifyRead)e.Record).BeforeRead(e);

            BeforeReadRecord?.Invoke(this, e);

            return e.SkipThisRecord;
        }

        /// <summary>
        /// Post process a record
        /// </summary>
        /// <param name="line">Record read</param>
        /// <param name="record">Type of record</param>
        /// <param name="lineChanged">Has the line been updated so that the engine switches to this version</param>
        /// <param name="lineNumber">Number of line in file</param>
        /// <returns>true if record to be skipped</returns>
        protected bool OnAfterReadRecord(string line, T record, bool lineChanged, int lineNumber)
        {
            var e = new Events.AfterReadEventArgs<T>(this, line, lineChanged, record, lineNumber);

            if (RecordInfo.NotifyRead) ((INotifyRead) record).AfterRead(e);

            AfterReadRecord?.Invoke(this, e);

            return e.SkipThisRecord;
        }

        /// <summary>
        /// Before a write is executed perform this check to see
        /// if we want to modify or reject the record.
        /// </summary>
        /// <param name="record">Instance to process</param>
        /// <param name="lineNumber">Number of line within file</param>
        /// <returns>true if record is to be dropped</returns>
        protected bool OnBeforeWriteRecord(T record, int lineNumber)
        {
            var e = new Events.BeforeWriteEventArgs<T>(this, record, lineNumber);

            if (RecordInfo.NotifyWrite) ((INotifyWrite) record).BeforeWrite(e);

            BeforeWriteRecord?.Invoke(this, e);

            return e.SkipThisRecord;
        }

        /// <summary>
        /// After we have written a record,  do we want to process it.
        /// </summary>
        /// <param name="line">Line that will be output</param>
        /// <param name="record">Record we are processing</param>
        /// <returns>Record to be written</returns>
        protected string OnAfterWriteRecord(string line, T record)
        {
            var e = new Events.AfterWriteEventArgs<T>(this, record, LineNumber, line);

            if (RecordInfo.NotifyWrite)
                ((INotifyWrite) record).AfterWrite(e);

            AfterWriteRecord?.Invoke(this, e);

            return e.RecordLine;
        }

        #region Implementation of IFileHelperEngine<T>

        public abstract T[] ReadFile(string fileName);
        public abstract T[] ReadFile(string fileName, int maxRecords);
        public abstract T[] ReadStream(TextReader reader);
        public abstract T[] ReadStream(TextReader reader, int maxRecords);
        public abstract T[] ReadString(string source);
        public abstract T[] ReadString(string source, int maxRecords);
        public abstract void WriteFile(string fileName, IEnumerable<T> records);
        public abstract void WriteFile(string fileName, IEnumerable<T> records, int maxRecords);
        public abstract void WriteStream(TextWriter writer, IEnumerable<T> records);
        public abstract void WriteStream(TextWriter writer, IEnumerable<T> records, int maxRecords);
        public abstract string WriteString(IEnumerable<T> records);
        public abstract string WriteString(IEnumerable<T> records, int maxRecords);
        public abstract void AppendToFile(string fileName, T record);
        public abstract void AppendToFile(string fileName, IEnumerable<T> records);

        #endregion
    }
}
