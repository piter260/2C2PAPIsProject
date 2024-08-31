<template>
    <div>
        <h1>{{ msg }}</h1>
        <!-- File Upload Section -->
        <div>
            <h1>Upload File</h1>
            <input type="file" @change="onFileChange" />
            <button @click="uploadFile">Upload</button>
        </div>

        <!-- Fetch by Currency -->
        <div>
            <h2>Fetch Transactions by Currency</h2>
            <input v-model="currency" placeholder="Enter Currency Code (e.g., USD)" />
            <button @click="fetchByCurrency">Fetch by Currency</button>
        </div>

        <!-- Fetch by Date Range -->
        <div>
            <h2>Fetch Transactions by Date Range</h2>
            <input v-model="startDate" placeholder="Start Date (YYYY-MM-DD)" />
            <input v-model="endDate" placeholder="End Date (YYYY-MM-DD)" />
            <button @click="fetchByDateRange">Fetch by Date Range</button>
        </div>

        <!-- Fetch by Status -->
        <div>
            <h2>Fetch Transactions by Status</h2>
            <input v-model="status" placeholder="Enter Status Code (e.g., A)" />
            <button @click="fetchByStatus">Fetch by Status</button>
        </div>

        <!-- Displaying the fetched transactions -->
        <ul v-if="transactions.length">
            <li v-for="transaction in transactions" :key="transaction.id">
                {{ transaction.transactionId }} - {{ transaction.currencyCode }} -
                {{ transaction.amount }} {{ transaction.currencyCode }} -
                {{ transaction.status }} - {{ transaction.transactionDate }}
            </li>
        </ul>
    </div>
</template>

<script>
    import axios from 'axios';
    export default {
        name: 'UploadFileComponent',
        data() {
            return {
                selectedFile: null,
                currency: '',        // To hold the currency input
                startDate: '',        // To hold the start date input
                endDate: '',          // To hold the end date input
                status: '',           // To hold the status input
                transactions: []      // To hold the fetched transactions
            };
        },
        methods: {
            onFileChange(event) {
                this.selectedFile = event.target.files[0];
            },
            async uploadFile() {
                if (!this.selectedFile) {
                    alert("Please select a file first");
                    return;
                }

                const fileExtension = this.selectedFile.name.split('.').pop().toLowerCase();

                if (fileExtension !== 'csv' && fileExtension !== 'xml') {
                    alert("Unknown format. Please upload a CSV or XML file.");
                    return;
                }

                const formData = new FormData();
                formData.append("file", this.selectedFile);

                try {
                    const response = await axios.post("https://localhost:7202/Transactions/file-upload", formData, {
                        headers: {
                            "Content-Type": "multipart/form-data"
                        }
                    });
                    alert("File uploaded successfully:");
                    console.log("File uploaded successfully:", response.data);
                } catch (error) {
                    console.error("Error uploading file:", error);
                }
            },
            async fetchByCurrency() {
                try {
                    const response = await axios.get(`https://localhost:7202/Transactions/get-transations-by-currency/${this.currency}`);
                    this.transactions = response.data;
                } catch (error) {
                    console.error("Error fetching transactions by currency:", error);
                }
            },
            async fetchByDateRange() {
                try {
                    const response = await axios.get(`https://localhost:7202/Transactions/get-transations-by-daterange`, {
                        params: {
                            startDate: this.startDate,
                            endDate: this.endDate
                        }
                    });
                    this.transactions = response.data;
                } catch (error) {
                    console.error("Error fetching transactions by date range:", error);
                }
            },
            async fetchByStatus() {
                try {
                    const response = await axios.get(`https://localhost:7202/Transactions/get-transations-by-status/${this.status}`);
                    this.transactions = response.data;
                } catch (error) {
                    console.error("Error fetching transactions by status:", error);
                }
            }
        }
    }
</script>

<style scoped>
    /* Add your styles here */
</style>
