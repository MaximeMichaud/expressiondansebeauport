<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link-title">{{ t(`routes.books.name`) }}</h1>
      <div class="content-grid__filters">
        <SearchInput v-model="searchValue"/>
      </div>
    </div>
    <Card :title="t('global.quickLinks')" class="card--row-content">
      <BtnLink
          :name="t('routes.books.children.add.name')"
          :path="{ name: 'books.children.add' }"
      />
    </Card>
    <Card>
      <DataTable
          :headers="bookHeader"
          :is-loading="booksAreLoading"
          :items="tableBooks"
          :search-value="searchValue"
          @delete="deleteBook"
      />
    </Card>
  </div>
</template>

<script lang="ts" setup>
import i18n from "@/i18n";
import {useI18n} from "vue3-i18n";
import {computed, onMounted, ref} from "vue";
import {useBookService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import Card from "@/components/layouts/items/Card.vue";
import SearchInput from "@/components/layouts/items/SearchInput.vue";
import DataTable from "@/components/layouts/items/DataTable.vue";
import BtnLink from "@/components/layouts/items/BtnLink.vue";
import {Book} from "@/types/entities";
import {Guid} from "@/types";

const {t} = useI18n()
const bookService = useBookService()

const allBooks = ref<Book[]>([]);
const searchValue = ref("");
const booksAreLoading = ref(false);

const tableBooks = computed(() => {
  let lang = i18n.getLocale()
  return allBooks.value.map((x: Book) => {
    return {
      id: x.id,
      name: (lang == 'fr' ? x.nameFr : x.nameEn) ?? '',
      isbn: x.isbn,
      author: x.author,
      editor: x.editor,
      actions: {
        update: {name: `books.children.edit`, params: {id: x.id}},
        delete: true
      }
    }
  }).sort((a: any, b: any) => {
    if (a.name < b.name) {
      return -1;
    }
    if (a.name > b.name) {
      return 1;
    }
    return 0;
  }) as Book[];
})

onMounted(async () => {
  await loadAllBooks();
});

async function loadAllBooks() {
  booksAreLoading.value = true;

  let books = await bookService.getAllBooks();
  if (books && books.length > 0) {
    allBooks.value = books;
  }
  booksAreLoading.value = false;
}

async function deleteBook(book: any) {
  if (book?.id == null || !Guid.isValid(book.id ?? ""))
    return

  let confirmDelete = confirm(t("validation.book.delete.confirmation"));
  if (!confirmDelete)
    return;

  let response = await bookService.deleteBook(book.id)
  if (response.succeeded) {
    allBooks.value = allBooks.value.filter(x => x.id !== book.id)
    notifySuccess(t('validation.book.delete.success'))
  } else {
    let errorMessages = response.getErrorMessages('validation.book.delete',
        'validation.book.delete.errorOccured')
    notifyError(errorMessages[0])
  }
}

const bookHeader = [
  {
    text: t("book.title"),
    value: 'name',
    sortable: true,
    width: 300,
  },
  {
    text: t("book.isbn"),
    value: "isbn",
    sortable: true,
    width: 150,
  },
  {
    text: t("book.author"),
    value: "author",
    sortable: true,
    width: 125,
  },
  {
    text: t("book.editor"),
    value: "editor",
    sortable: true,
    width: 125,
  },
  {
    text: t("global.table.actions"),
    value: "actions",
    width: 75
  },
];

</script>
