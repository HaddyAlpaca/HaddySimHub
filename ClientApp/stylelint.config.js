import standardScss from 'stylelint-config-standard-scss';

export default {
  extends: ['stylelint-config-standard-scss'],
  rules: {
    'color-function-notation': 'modern',
    'alpha-value-notation': 'percentage',
    'declaration-block-single-line-max-declarations': 3,
  }
};