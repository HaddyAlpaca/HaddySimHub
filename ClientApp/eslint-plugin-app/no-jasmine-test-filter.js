module.exports = {
    meta: {
        type: 'problem',
        docs: {
            description: 'disallow jasmine test filter, like `fdescribe, fit, xit, xdescribe` calls',
            category: 'Best Practices',
            recommended: false,
        },
        schema: [], // no options
    },
    create(context) {
        return {
            CallExpression(node) {
                const notAllowedWords = ['fdescribe', 'fit', 'xit', 'xdescribe'];

                if (node.callee.type === 'Identifier' && notAllowedWords.includes(node.callee.name)) {
                    context.report({
                        node,
                        message: `The method call "${node.callee.name}" is disallowed in .ts files.`,
                    });
                }
            },
        };
    },
};
