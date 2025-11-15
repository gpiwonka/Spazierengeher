using System;
using System.Collections.Generic;
using System.Text;

namespace Spazierengeher.Services
{
    public class DummyStepCounterService : IStepCounterService
    {
        private Timer _stepTimer;
        private int _currentSteps = 0;
        private Random _random = new Random();
        private bool _isRunning = false;

        public event EventHandler<int> StepCountChanged;

        public Task<bool> CheckPermissionAsync()
        {
            // Dummy gibt immer true zurück
            return Task.FromResult(true);
        }

        public Task<bool> RequestPermissionAsync()
        {
            // Dummy gibt immer true zurück
            return Task.FromResult(true);
        }

        public Task StartCountingAsync()
        {
            if (_isRunning)
            {
                return Task.CompletedTask;
            }

            _isRunning = true;
            _currentSteps = 0;

            // Simuliere Schritte alle 2-5 Sekunden
            _stepTimer = new Timer(SimulateSteps, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));

            System.Diagnostics.Debug.WriteLine("🎭 Dummy-Schrittzähler gestartet");
            return Task.CompletedTask;
        }

        public Task StopCountingAsync()
        {
            _isRunning = false;
            _stepTimer?.Dispose();
            _stepTimer = null;

            System.Diagnostics.Debug.WriteLine($"🎭 Dummy-Schrittzähler gestoppt bei {_currentSteps} Schritten");
            return Task.CompletedTask;
        }

        public Task<int> GetStepCountAsync()
        {
            return Task.FromResult(_currentSteps);
        }

        private void SimulateSteps(object state)
        {
            if (!_isRunning)
                return;

            // Generiere realistische Schrittzahlen
            // Menschen machen durchschnittlich 2 Schritte pro Sekunde beim Gehen
            // Wir simulieren Intervalle von 2-5 Sekunden, also 4-10 Schritte pro Interval
            int newSteps = _random.Next(4, 11);
            _currentSteps += newSteps;

            System.Diagnostics.Debug.WriteLine($"🎭 Dummy: +{newSteps} Schritte (Gesamt: {_currentSteps})");

            // Event auf Main-Thread auslösen
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StepCountChanged?.Invoke(this, _currentSteps);
            });
        }
    }
}
