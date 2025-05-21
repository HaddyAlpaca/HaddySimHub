/**
  * Configuration for eslint
  *
  * Run with:
  * - `ng lint`
  * - `node node_modules\eslint\bin\eslint.js --debug .`
  * - Visual Studio Code: install 'vscode-eslint' extension
  * - Visual Studio 2019/2022: Tools > Options > TextEditor > Javascript/TypeScript > Linting > General
  *             "[x] Enable ESLint"
  *             "[x] Lint all files in project, even closed files"
  *
  */

/*@ts-check*/
import { fixupPluginRules } from '@eslint/compat';

import globals from 'globals';
import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';
import angulareslint from 'angular-eslint';

import stylisticTsPlugin from '@stylistic/eslint-plugin-ts';
import deprecationPlugin from 'eslint-plugin-deprecation';
import jsDocPlugin from 'eslint-plugin-jsdoc';
import preferArrowPlugin from 'eslint-plugin-prefer-arrow';
import pluginRxjs from 'eslint-plugin-rxjs';
import pluginAngularRxjs from 'eslint-plugin-rxjs-angular';

export default tseslint.config(
    {
      files: ['*.js', '*.cjs'],
      extends: [
        // Don't typecheck JS files
        tseslint.configs.disableTypeChecked
      ],
      rules: {
        'quotes': ['error', 'single', { 'allowTemplateLiterals': true }],
        // Disable type-aware rules
        'deprecation/deprecation': 'off',
      }
    },
    {
        files: ['**/*.ts'],
        extends: [
            // Apply the recommended core rules
            eslint.configs.recommended,
            // Apply the recommended TypeScript rules
            ...tseslint.configs.recommendedTypeChecked,
            // Optionally apply stylistic rules from typescript-eslint that improve code consistency
            ...tseslint.configs.stylisticTypeChecked,
            // Apply the recommended Angular rules
            ...angulareslint.configs.tsRecommended,
        ],
        plugins: {
            'deprecation': fixupPluginRules(deprecationPlugin),
            '@stylistic/ts': stylisticTsPlugin,
            'jsdoc': fixupPluginRules(jsDocPlugin),
            'prefer-arrow': fixupPluginRules(preferArrowPlugin),
            'rxjs': fixupPluginRules(pluginRxjs),
            'rxjs-angular': fixupPluginRules(pluginAngularRxjs),
        },
        languageOptions: {
            globals: {
                ...globals.browser,
                ...globals.jasmine,
                ...globals.es6,
            },
            parserOptions: {
                // Enable typed liniting
                projectService: true,
                tsconfigRootDir: import.meta.dirname,
            }
        },
        // Set the custom processor which will allow us to have our inline Component templates extracted
        // and treated as if they are HTML files (and therefore have the .html config below applied to them)
        processor: angulareslint.processInlineTemplates,
        // Override specific rules for TypeScript files (these will take priority over the extended configs above)
        // The list below is ordered alphabetically.
        // This list gradually evolved overtime and is largy based on the old 'plugin:@angular-eslint/ng-cli-compat', and 'plugin:@angular-eslint/ng-cli-compat--formatting-add-on'.
        rules: {
            // @angular-eslint
            '@angular-eslint/component-selector': [
                'error',
                {
                    'type': 'element',
                    'prefix': 'app',
                    'style': 'kebab-case'
                }
            ],
            '@angular-eslint/directive-selector': [
                'error',
                {
                    'type': 'attribute',
                    'prefix': 'app',
                    'style': 'camelCase'
                }
            ],
            '@angular-eslint/no-conflicting-lifecycle': 'error',
            '@angular-eslint/use-lifecycle-interface': 'error',
            '@angular-eslint/prefer-on-push-component-change-detection': 'error',

            // @stylistic/ts
            '@stylistic/ts/semi': ['error', 'always'],
            '@stylistic/ts/comma-dangle': [
                // matches https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1413.md
                'error',
                'always-multiline'
            ],
            '@stylistic/ts/indent': ['error', 2],
            '@stylistic/ts/member-delimiter-style': [
                'error',
                {
                    'multiline': {
                    'delimiter': 'semi',
                    'requireLast': true
                    },
                    'singleline': {
                    'delimiter': 'semi',
                    'requireLast': false
                    }
                }
            ],
            '@stylistic/ts/quotes': [
                'error',
                'single',
                { 'allowTemplateLiterals': true }
            ],
            '@stylistic/ts/type-annotation-spacing': 'error',

            // @typescript-eslint
            '@typescript-eslint/await-thenable': 'error',
            '@typescript-eslint/consistent-type-definitions': 'error',
            '@typescript-eslint/explicit-member-accessibility': 'error',
            '@typescript-eslint/explicit-function-return-type': 'error',
            '@typescript-eslint/member-ordering': [
                'error',
                { 'default': { 'memberTypes': 'never' } }
            ],
            '@typescript-eslint/naming-convention': [
                'error',
                {
                    'selector': ['variableLike'],
                    'format': ['camelCase'],
                    'leadingUnderscore': 'allow'
                },
                {
                    'selector': ['variableLike'],
                    'modifiers': ['exported', 'const'],
                    'format': ['UPPER_CASE', 'camelCase']
                },
                {
                    'selector': ['property', 'parameterProperty', 'accessor', 'enumMember'],
                    'modifiers': ['private'],
                    'format': ['camelCase'],
                    'leadingUnderscore': 'require'
                },
                {
                    'selector': ['enum' ],
                    'format': [ 'PascalCase' ],
                    'leadingUnderscore': 'allow'
                }
            ],
            '@typescript-eslint/no-explicit-any': 'error',
            '@typescript-eslint/no-floating-promises': 'error',
            '@typescript-eslint/no-misused-promises': 'error',
            '@typescript-eslint/no-non-null-assertion': 'error',
            '@typescript-eslint/no-shadow': ['error'],
            '@typescript-eslint/no-unused-expressions': 'error',
            '@typescript-eslint/no-unused-vars': ['error', { 'argsIgnorePattern': '^_' }],
            '@typescript-eslint/prefer-for-of': 'error',
            '@typescript-eslint/prefer-function-type': 'error',
            'require-await': 'off',
            '@typescript-eslint/require-await': 'error',
            'no-return-await': 'off',
            '@typescript-eslint/return-await': 'error',
            '@typescript-eslint/unbound-method': [
                'error',
                { 'ignoreStatic': true }
            ],
            '@typescript-eslint/unified-signatures': 'error',
            '@typescript-eslint/prefer-readonly': 'error',

            // eslint-plugin-deprecation
            'deprecation/deprecation': 'error',

            // js-doc
            'jsdoc/tag-lines': 'error',
            'jsdoc/no-types': 'error',
            'jsdoc/check-alignment': 'error',

            // prefer-arrow
            'prefer-arrow/prefer-arrow-functions': 'error',

            // rxjs-angular
            'rxjs-angular/prefer-takeuntil': [
                'error',
                {
                    'alias': ['untilDestroyed'],
                    'checkComplete': true,
                    'checkDecorators': ['Component', 'Directive'],
                    'checkDestroy': false
                }
            ],
            'rxjs/no-unsafe-takeuntil': [
                'error',
                {
                    'alias': ['untilDestroyed']
                }
            ],


            // eslint - https://eslint.org/docs/latest/rules
            'arrow-body-style': 'error',
            'brace-style': [
                'error',
                '1tbs'
            ],
            'comma-dangle': 'off', // see @typescript-eslint/comma-dangle
            'curly': 'error',
            'eol-last': 'error',
            'eqeqeq': ['error', 'smart'],
            'guard-for-in': 'error',
            'max-len': [
                'error',
                {
                    'code': 200
                }
            ],
            'new-parens': 'error',
            'no-bitwise': 'error',
            'no-caller': 'error',
            'no-console': [
                'error',
                {
                    'allow': [
                    'log',
                    'warn',
                    'dir',
                    'timeLog',
                    'assert',
                    'clear',
                    'count',
                    'countReset',
                    'group',
                    'groupEnd',
                    'table',
                    'dirxml',
                    'error',
                    'groupCollapsed',
                    'Console',
                    'profile',
                    'profileEnd',
                    'timeStamp',
                    'context'
                    ]
                }
            ],
            'no-debugger': 'error',
            'no-eval': 'error',
            'no-new-wrappers': 'error',
            'no-restricted-imports': [
                'error',
                {
                    'name': 'rxjs/Rx',
                    'message': 'Please import directly from \'rxjs\' instead'
                }
            ],
            'no-undef': 'error',
            'no-throw-literal': 'error',
            'no-trailing-spaces': 'error',
            'no-undef-init': 'error',
            'no-var': 'error',
            'object-shorthand': 'error',
            'one-var': ['error', 'never'],
            'prefer-arrow-callback': 'error',
            'prefer-const': 'error',
            'quote-props': ['error', 'as-needed'],
            'radix': 'error',
            'space-before-function-paren': [
                'error',
                {
                    'anonymous': 'never',
                    'asyncArrow': 'always',
                    'named': 'never'
                }
            ],
        },
    },

    {
        // Everything in this config object targets our HTML files (external templates,
        // and inline templates as long as we have the `processor` set on our TypeScript config above)
        files: ['**/*.html'],
        extends: [
            // Apply the recommended Angular template rules
            ...angulareslint.configs.templateRecommended,
            // Apply the Angular template rules which focus on accessibility of our apps
            // ...angulareslint.configs.templateAccessibility,
        ],
        rules: {
            /**
             * Any template/HTML related rules you wish to use/reconfigure over and above the
             * recommended set provided by the @angular-eslint project would go here.
             */
            '@angular-eslint/template/prefer-control-flow': 'error',
        },
    },

    {
      files: ['**/*.harness.ts'],
      rules: {
        'no-restricted-imports': [
          'error',
          {
            // Patterns are gitignore style patterns.
            'patterns': ['*', '!*/', '!*.harness', '!@angular/cdk/testing', '!@angular/cdk/testing/**']
          }
        ],
      }
    },

    {
        files: ['**/*.spec.ts'],
        rules: {
            '@typescript-eslint/no-non-null-assertion': 'off',
            '@typescript-eslint/unbound-method': 'off',
            '@angular-eslint/prefer-on-push-component-change-detection': 'off',
        }
    }
);
