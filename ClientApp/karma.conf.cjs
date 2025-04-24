// karma.conf.js
module.exports = function (config) {
    config.set({
      // Base path that will be used to resolve all patterns (eg. files, exclude)
      basePath: '',
      frameworks: ['jasmine'],
      plugins: [
        require('karma-jasmine'),
        require('karma-chrome-launcher'),
        require('karma-coverage'),
        require('karma-jasmine-order-reporter'),
        require('karma-junit-reporter'),
        require('karma-sabarivka-reporter'),
        require('karma-spec-reporter'),
      ],
      client: {
        clearContext: false
      },
      coverageReporter: {
        dir: require('path').join(__dirname, './coverage'),
        include: [
          // These are the include options for karma-sabarivka-reporter
          // Specify include pattern(s) first
          'src/**/*.(ts|js)',
          // Then specify "do not include" patterns (note `!` sign on the beginning of each statement)
          '!src/**/*.spec.(ts|js)',
          '!src/main.(ts|js)',
          '!src/environments/**/*.(ts|js)',
        ],
        reporters: [
          { type: 'html', subdir: 'report-html' },
        ],
        fixWebpackSourcePaths: true,
      },
      junitReporter: {
        outputDir: require('path').join(__dirname, './junit'),
        outputFile: 'TESTS.xml'
      },
      reporters: ['spec', 'sabarivka', 'jasmine-order', 'junit', 'coverage'],
      specReporter: {
        maxLogLines: 5,             // limit number of lines logged per test
        suppressSummary: false,      // do not print summary
        suppressErrorSummary: true, // do not print error summary
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
  