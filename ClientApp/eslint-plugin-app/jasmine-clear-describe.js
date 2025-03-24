module.exports = {
    meta: {
        type: 'problem',
        docs: {
            description: 'jasmine describe should have ` tests` at the end of its description, to make it easier to find the spec file',
            category: 'Best Practices',
            recommended: false,
        },
        schema: [], // no options
    },
    create(context) {
        return {
            CallExpression(node) {
                if (node.callee.type === 'Identifier' && node.callee.name === 'describe') {
                    const [, description] = node.arguments;
                    if (description.type === 'Literal' && !description.value.endsWith(' tests')) {
                        context.report({
                            node,
                            message: 'Jasmine describe should have ` tests` at the end of its description, to make it easier to find the spec file',
                        });
                    }
                }
            },
        };
    },
  };
  