/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Areas/**/*.cshtml',
        './Areas/**/*.cs',
        './Views/**/*.cshtml',
        './Pages/**/*.cshtml',
        './Controllers/**/*.cs',
        './Middleware/**/*.cs',
    ],
    theme: {
        extend: {
            fontFamily: {
                'poppins': ['Poppins', 'sans-serif'],
            },
            colors: {
                'admin-blue': '#1d4ed8',
                'admin-blue-light': '#2563eb',
                'admin-blue-dark': '#1e40af',
                'sidebar-blue': '#1C4CD2',
            }
        }
    },
    plugins: [],
}
