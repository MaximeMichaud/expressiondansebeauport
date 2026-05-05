import eslint from '@eslint/js';
import eslintPluginVue from 'eslint-plugin-vue';
import vuejsAccessibility from 'eslint-plugin-vuejs-accessibility';
import globals from 'globals';
import typescriptEslint from 'typescript-eslint';

const accessibilityRules = vuejsAccessibility.configs['flat/recommended'][0].rules;

export default typescriptEslint.config(
  {
    ignores: ['**/dist/**', '**/node_modules/**'],
  },
  {
    extends: [
      eslint.configs.recommended,
      ...typescriptEslint.configs.recommended,
      ...eslintPluginVue.configs['flat/essential'],
    ],
    files: ['**/*.{js,mjs,cjs,ts,mts,cts,vue}'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: {
        ...globals.browser,
        ...globals.node,
      },
      parserOptions: {
        parser: typescriptEslint.parser,
      },
    },
    rules: {
      'no-console': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
      'no-debugger': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
      'vue/multi-word-component-names': 'off',
      '@typescript-eslint/no-explicit-any': 'off',
      '@typescript-eslint/no-empty-object-type': 'off',
      'vue/require-toggle-inside-transition': 'off',
    },
  },
  {
    files: [
      'src/components/blocks/renderers/**/*.vue',
      'src/components/forms/**/*.vue',
      'src/components/layouts/PublicLayout.vue',
      'src/components/layouts/AuthenticationLayout.vue',
      'src/components/layouts/items/CookieBanner.vue',
      'src/components/layouts/items/PublicFooter.vue',
      'src/components/navigation/PublicNavbar.vue',
      'src/views/Login.vue',
      'src/views/ForgotPassword.vue',
      'src/views/ResetPassword.vue',
      'src/views/TwoFactor.vue',
      'src/views/public/**/*.vue',
    ],
    plugins: {
      'vuejs-accessibility': vuejsAccessibility,
    },
    rules: {
      ...accessibilityRules,
      'vuejs-accessibility/label-has-for': 'off',
      'vuejs-accessibility/no-static-element-interactions': 'off',
    },
  },
);
