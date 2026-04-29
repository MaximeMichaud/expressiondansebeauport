<template>
  <span v-bind="$attrs"
        :aria-hidden="title ? null : 'true'"
        :aria-label="title"
        class="material-design-icon tiktok-icon"
        role="img"
        @click="$emit('click', $event)">
    <svg :fill="fillColor"
         class="material-design-icon__svg"
         :width="size"
         :height="size"
         viewBox="0 0 24 24">
      <path d="M12.53,0.02C13.84,0 15.14,0.01 16.44,0C16.52,1.53 17.07,3.09 18.19,4.17C19.31,5.28 20.89,5.79 22.43,5.96V9.99C20.99,9.94 19.54,9.64 18.23,9.02C17.66,8.76 17.13,8.43 16.61,8.09C16.6,11.01 16.62,13.93 16.59,16.84C16.51,18.24 16.05,19.63 15.24,20.78C13.93,22.7 11.66,23.95 9.33,23.99C7.9,24.07 6.47,23.68 5.25,22.96C3.23,21.77 1.81,19.59 1.6,17.25C1.58,16.75 1.57,16.25 1.59,15.76C1.77,13.86 2.71,12.04 4.17,10.8C5.83,9.36 8.15,8.67 10.32,9.08C10.34,10.56 10.28,12.04 10.28,13.52C9.29,13.2 8.13,13.29 7.26,13.89C6.63,14.3 6.15,14.93 5.9,15.64C5.69,16.15 5.75,16.71 5.76,17.25C6,18.89 7.58,20.27 9.26,20.12C10.38,20.11 11.45,19.46 12.03,18.51C12.22,18.18 12.43,17.84 12.44,17.45C12.54,15.66 12.5,13.88 12.51,12.09C12.52,8.06 12.5,4.05 12.53,0.02Z">
        <title v-if="title">{{ title }}</title>
      </path>
    </svg>
  </span>
</template>

<script>
export default {
  name: "TiktokIcon",
  emits: ["click"],
  props: {
    title: {
      type: String,
    },
    fillColor: {
      type: String,
      default: "currentColor"
    },
    size: {
      type: Number,
      default: 24
    }
  }
}
</script>
