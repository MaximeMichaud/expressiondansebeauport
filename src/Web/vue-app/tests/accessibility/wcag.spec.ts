import {expect, type Page, type TestInfo, test} from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import type {Result} from 'axe-core'

const wcagTags = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa']

async function expectNoWcagViolations(page: Page, testInfo: TestInfo) {
  const results = await new AxeBuilder({page})
    .withTags(wcagTags)
    .analyze()

  await testInfo.attach('axe-results.json', {
    body: JSON.stringify(results, null, 2),
    contentType: 'application/json',
  })

  expect(formatViolations(results.violations)).toEqual([])
}

function formatViolations(violations: Result[]) {
  return violations.map(violation => ({
    id: violation.id,
    impact: violation.impact,
    help: violation.help,
    nodes: violation.nodes.map(node => ({
      target: node.target,
      failureSummary: node.failureSummary,
    })),
  }))
}

test.describe('accessibilité WCAG', () => {
  test('la page d’accueil expose un skip link, un main et aucune violation axe', async ({page}, testInfo) => {
    await page.goto('/')
    await expect(page.locator('main#main-content')).toBeVisible()

    const skipLink = page.getByRole('link', {name: 'Passer au contenu principal'})
    await page.keyboard.press('Tab')
    await expect(skipLink).toBeFocused()
    await page.keyboard.press('Enter')
    await expect(page.locator('main#main-content')).toBeFocused()

    await expectNoWcagViolations(page, testInfo)
  })

  test('la navigation publique est utilisable au clavier', async ({page}, testInfo) => {
    await page.goto('/')

    const nav = page.getByRole('navigation', {name: 'Navigation principale'})
    await expect(nav).toBeVisible()

    const menuButton = nav.getByRole('button', {name: /menu/i})
    if (await menuButton.isVisible()) {
      await menuButton.focus()
      await page.keyboard.press('Enter')
      await expect(menuButton).toHaveAttribute('aria-expanded', 'true')
      await page.keyboard.press('Escape')
      await expect(menuButton).toHaveAttribute('aria-expanded', 'false')
    }

    await expectNoWcagViolations(page, testInfo)
  })

  test('la connexion admin n’a pas de violation WCAG détectable automatiquement', async ({page}, testInfo) => {
    await page.goto('/admin/connexion')

    await expect(page.getByLabel(/nom d'utilisateur|courriel|username/i)).toBeVisible()
    await expect(page.getByLabel(/mot de passe|password/i)).toBeVisible()

    await expectNoWcagViolations(page, testInfo)
  })
})
