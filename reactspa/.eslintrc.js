module.exports = {
    extends: [
        'plugin:react/recommended',
        'plugin:@typescript-eslint/recommended',
        'plugin:prettier/recommended',
    ],
    rules: {
        'import/no-extraneous-dependencies': 'off',
        'import/no-unresolved': 'error',
        'react/react-in-jsx-scope': 'off'
    },
    parserOptions: {
        ecmaVersion: 2020,
        sourceType: 'module',
        project: './tsconfig.json',
        tsconfigRootDir: __dirname,
        createDefaultProgram: true
    },
    settings: {
        node: {},
        webpack: {
            config: require.resolve('./configs/webpack.config.dev.ts')
        },
        typescript: {},
    },
    'import/parsers': {
        '@typescript-eslint/parser': ['.ts', '.tsx']
    }
};
