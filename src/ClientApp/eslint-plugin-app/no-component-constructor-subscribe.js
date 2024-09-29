module.exports = {
    create(context) {
      return {
        // Target class declarations
        ClassDeclaration(node) {
          // Check if the class has an @Component applied (assumes imported from Angular)
          const isAngularComponent = node.decorators && node.decorators.some(decorator => 
            decorator.expression.callee && decorator.expression.callee.name === 'Component'
          );
  
          if (isAngularComponent) {
            node.body.body.forEach(method => {
              if (method.kind === 'constructor') {
                const sourceCode = context.getSourceCode().getText(method);
  
                if (sourceCode.includes('subscribe')) {
                  context.report({
                    node: method,
                    message: `The word "subscribe" is disallowed in @Component class constructors.
                        Use Angular lifecycle hooks instead. E.g. move the subscription to ngOnInit.`,
                  });
                }
              }
            });
          }
        },
      };
    },
  };