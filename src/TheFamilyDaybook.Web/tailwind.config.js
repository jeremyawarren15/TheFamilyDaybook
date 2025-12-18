/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}",
    "./wwwroot/**/*.html"
  ],
  theme: {
    extend: {
      colors: {
        'creamy': {
          DEFAULT: '#FEFBF7',
          'light': '#FAF8F3',
          'dark': '#F5F1E8'
        },
        'burgundy': {
          DEFAULT: '#991B1B',
          'light': '#B91C1C',
          'dark': '#7F1D1D',
          'hover': '#DC2626'
        }
      },
      backgroundColor: {
        'primary': '#FEFBF7',
        'card': '#FFFFFF'
      }
    },
  },
  plugins: [],
}

