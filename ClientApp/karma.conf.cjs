// karma.conf.js
module.exports = function (config) {
    config.set({
      // Base path that will be used to resolve all patterns (eg. files, exclude)
      basePath: '',
      
      // Frameworks to use
      frameworks: ['jasmine', '@angular-devkit/build-angular'],
      
      // List of plugins to load
      plugins: [
        require('karma-jasmine'),
        require('karma-chrome-launcher'),
        require('@angular-devkit/build-angular/plugins/karma'),
        require('karma-spec-reporter')
      ],
      
      // Client configuration: keeping the Jasmine Spec Runner output visible in the browser
      client: {
        clearContext: false
      },
      reporters: ['spec'],

      specReporter: {
        maxLogLines: 5,             // limit number of lines logged per test
        suppressSummary: false,      // do not print summary
        suppressErrorSummary: false, // do not print error summary
        suppressFailed: false,      // do not print information about failed tests
        suppressPassed: false,      // do not print information about passed tests
        suppressSkipped: true,      // do not print information about skipped tests
        showBrowser: false,         // print the browser for each spec
        showSpecTiming: true,       // print the time elapsed for each spec
      },
      
      // Web server port
      port: 9876,
      
      // Enable colors in the output (reporters and logs)
      colors: true,
      
      // Level of logging
      logLevel: config.LOG_INFO,
      
      // Watch file changes and re-run tests automatically
      autoWatch: true,
      
      // Start these browsers; here, Chrome is used
      browsers: ['Chrome'],
      
      // Continuous Integration mode:
      // if true, Karma captures browsers, runs the tests, and exits
      singleRun: false,
      
      // Concurrency level: how many browser instances should be started simultaneously
      concurrency: Infinity,
    });
  };
  