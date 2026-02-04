<template>
    <div class="content-grid content-grid--subpage">
        <Breadcrumbs :title="t(`routes.books.children.edit.name`)" />
        <BackLinkTitle :title="t(`routes.books.children.edit.name`)" />
        <Card>
            <BookForm :book="book" @formSubmit="handleSubmit" />
        </Card>
    </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n";
import {notifyError, notifySuccess} from "@/notify";
import {useBookService} from "@/inversify.config";
import {useRouter} from "vue-router";
import {Book} from "@/types/entities";
import Breadcrumbs from "@/components/layouts/items/Breadcrumbs.vue";
import BackLinkTitle from "@/components/layouts/items/BackLinkTitle.vue";
import BookForm from "@/components/books/BookForm.vue";
import Card from "@/components/layouts/items/Card.vue";
import {ref} from "vue";
import {IEditBookRequest} from "@/types/requests/editBookRequest";

// eslint-disable-next-line no-undef
const props = defineProps<{
    id: string
}>();

const {t} = useI18n()
const router = useRouter();
const bookService = useBookService();

const book = ref<Book>(await bookService.getBook(props.id));

async function handleSubmit(book : Book) {
    let succeededOrNotResponse = await bookService.editBook(book as IEditBookRequest)
    if (succeededOrNotResponse && succeededOrNotResponse.succeeded) {
        notifySuccess(t('validation.book.edit.success'))
        setTimeout(() => {
            router.back();
        }, 1500);
    } else {
        let errorMessages = succeededOrNotResponse.getErrorMessages('validation.book.edit');
        if (errorMessages.length == 0)
            notifyError(t('validation.book.edit.errorOccured'))
        else
            notifyError(errorMessages[0])
    }
}
</script>
