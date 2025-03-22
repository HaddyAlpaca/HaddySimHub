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
      port: 9876,
      colors: true,
      logLevel: config.LOG_INFO,
      autoWatch: true,
      browsers: ['ChromeHeadless'],
      singleRun: false,
      concurrency: Infinity,
    });
  };
  