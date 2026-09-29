using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Validator
{
    internal sealed class TriValidationBackgroundJob
    {
        private readonly IEnumerator<TriValidationScanStep> _routine;

        private int _progressId;
        private bool _cancelRequested;
        private bool _finished;

        private TriValidationBackgroundJob(string title, IEnumerator<TriValidationScanStep> routine)
        {
            _routine = routine;

            _progressId = Progress.Start(title, null, Progress.Options.Managed);
            Progress.RegisterCancelCallback(_progressId, OnCancelRequested);

            EditorApplication.update += Tick;
        }

        public static TriValidationBackgroundJob Start(string title, IEnumerator<TriValidationScanStep> routine)
        {
            return new TriValidationBackgroundJob(title, routine);
        }

        public void Cancel()
        {
            if (_finished)
            {
                return;
            }

            _cancelRequested = true;
            Finish(Progress.Status.Canceled);
        }

        private bool OnCancelRequested()
        {
            _cancelRequested = true;
            return true;
        }

        private void Tick()
        {
            if (_finished)
            {
                return;
            }

            if (_cancelRequested)
            {
                Finish(Progress.Status.Canceled);
                return;
            }

            bool moved;
            try
            {
                moved = _routine.MoveNext();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Finish(Progress.Status.Failed);
                return;
            }

            if (!moved)
            {
                Finish(Progress.Status.Succeeded);
                return;
            }

            var step = _routine.Current;
            if (_progressId >= 0 && !step.Equals(default))
            {
                Progress.Report(_progressId, step.index, step.total, step.info);
            }
        }

        private void Finish(Progress.Status status)
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            EditorApplication.update -= Tick;

            _routine?.Dispose();

            if (_progressId >= 0 && Progress.Exists(_progressId))
            {
                if (status == Progress.Status.Canceled)
                {
                    Progress.Remove(_progressId);
                }
                else
                {
                    Progress.Finish(_progressId, status);
                }
            }

            _progressId = -1;
        }
    }
}